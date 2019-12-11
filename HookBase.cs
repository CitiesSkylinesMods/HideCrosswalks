using System;
using UnityEngine;
using System.Reflection;

namespace Kian.Patch {
    public abstract class HookBase : MonoBehaviour {
        private RedirectCallsState State = null;
        public abstract MethodInfo From { get; }
        public abstract MethodInfo To { get; }

        public void Hook() {
            if(State != null) {
                Debug.Log("already hooked.");
                return;
            }
            MethodInfo from = From;
            MethodInfo to = To;
            Debug.Log($"Hooking {STR(from)} to {STR(to)} ...");
            if (From == null || To == null) {
                Debug.LogError("hooking failed!");
                return;
            }
            State = RedirectionHelper.RedirectCalls(from, to);
        }
        public void UnHook() {
            if (State != null) {
                MethodInfo from = From;
                Debug.Log($"UnHooking {STR(from)} ...");
                RedirectionHelper.RevertRedirect(from, State);
                State = null;
            }
        }

        public string STR(MethodInfo info)=> info == null? "null": $"{info?.DeclaringType?.FullName} :: {info}";
    }
}
