using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Jailbreak.Shared;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Memory;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace JailbreakCore;

public class Hooks(ISwiftlyCore core)
{
    private readonly ISwiftlyCore _Core = core;
    private readonly List<Func<DamageHookContext, HookResult>> _takeDamageListeners = new();
    private readonly object _takeDamageListenersLock = new();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool TakeDamageDelegate(nint entity, nint damageInfo);

    private IUnmanagedFunction<TakeDamageDelegate>? _takeDamage;
    private Guid _takeDamageHookId;

    public void Register()
    {
        var address = _Core.GameData.GetSignature("CBaseEntity::TakeDamage");
        if (address == IntPtr.Zero)
        {
            _Core.Logger.LogWarning("CBaseEntity::TakeDamage signature missing - hook skipped");
            return;
        }

        _takeDamage = _Core.Memory.GetUnmanagedFunctionByAddress<TakeDamageDelegate>(address);
        _takeDamageHookId = _takeDamage.AddHook(next => (pEntity, pInfo) =>
        {
            unsafe
            {
                var info = (CTakeDamageInfo*)pInfo;

                var victimPlayer = FindPlayerByPawnAddress(pEntity);
                var attackerPlayer = ResolveHandle(info->Attacker);

                if (victimPlayer != null && attackerPlayer != null)
                {
                    var jbVictim = JailbreakCore.JBPlayerManagement.GetOrCreate(victimPlayer);
                    var jbAttacker = JailbreakCore.JBPlayerManagement.GetOrCreate(attackerPlayer);

                    var managedCopy = *info;

                    var lrResult = JailbreakCore.LastRequest.OnTakeDamage(managedCopy, jbAttacker, jbVictim);
                    var boxResult = JailbreakCore.Extensions.OnBoxActive(managedCopy, jbAttacker, jbVictim);
                    var externalResult = DispatchTakeDamage(ref managedCopy, jbAttacker, jbVictim);

                    *info = managedCopy;

                    if (lrResult == HookResult.Handled || boxResult == HookResult.Handled || externalResult == HookResult.Handled)
                        return false;
                }
            }

            return next()(pEntity, pInfo);
        });
    }

    public void Unregister()
    {
        if (_takeDamage != null && _takeDamageHookId != Guid.Empty)
        {
            _takeDamage.RemoveHook(_takeDamageHookId);
            _takeDamageHookId = Guid.Empty;
        }

        lock (_takeDamageListenersLock)
        {
            _takeDamageListeners.Clear();
        }
    }

    public IDisposable SubscribeTakeDamage(Func<DamageHookContext, HookResult> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        lock (_takeDamageListenersLock)
        {
            _takeDamageListeners.Add(callback);
        }

        return new HookSubscription(this, callback);
    }

    private IPlayer? FindPlayerByPawnAddress(nint address)
    {
        foreach (var player in _Core.PlayerManager.GetAllPlayers())
        {
            try
            {
                var pawn = player.PlayerPawn;
                if (pawn != null && pawn.Address == address)
                    return player;
            }
            catch (NullReferenceException)
            {
                // Player state not fully initialized, skip
                continue;
            }
        }

        return null;
    }

    private IPlayer? ResolveHandle(CHandle<CEntityInstance> handle)
    {
        if (!handle.IsValid)
            return null;

        var entity = handle.Value;
        if (entity == null)
            return null;

        foreach (var player in _Core.PlayerManager.GetAllPlayers())
        {
            try
            {
                if (player.PlayerPawn?.Address == entity.Address ||
                    player.Controller?.Address == entity.Address)
                {
                    return player;
                }
            }
            catch (NullReferenceException)
            {
                // Player state not fully initialized, skip
                continue;
            }
        }

        return null;
    }

    private HookResult DispatchTakeDamage(ref CTakeDamageInfo info, JBPlayer attacker, JBPlayer victim)
    {
        List<Func<DamageHookContext, HookResult>> snapshot;

        lock (_takeDamageListenersLock)
        {
            if (_takeDamageListeners.Count == 0)
                return HookResult.Continue;

            snapshot = new List<Func<DamageHookContext, HookResult>>(_takeDamageListeners);
        }

        var context = new DamageHookContext(info, attacker, victim);
        var finalResult = HookResult.Continue;

        foreach (var listener in snapshot)
        {
            try
            {
                var result = listener(context);
                if (result == HookResult.Handled)
                    finalResult = HookResult.Handled;
            }
            catch (Exception ex)
            {
                _Core.Logger.LogError(ex, "Error executing TakeDamage hook subscriber.");
            }
        }

        info = context.Info;
        return finalResult;
    }

    private void Unsubscribe(Func<DamageHookContext, HookResult> callback)
    {
        lock (_takeDamageListenersLock)
        {
            _takeDamageListeners.Remove(callback);
        }
    }

    private sealed class HookSubscription : IDisposable
    {
        private Hooks? _owner;
        private Func<DamageHookContext, HookResult>? _callback;

        public HookSubscription(Hooks owner, Func<DamageHookContext, HookResult> callback)
        {
            _owner = owner;
            _callback = callback;
        }

        public void Dispose()
        {
            var owner = Interlocked.Exchange(ref _owner, null);
            var callback = Interlocked.Exchange(ref _callback, null);

            if (owner != null && callback != null)
            {
                owner.Unsubscribe(callback);
            }
        }
    }
}
