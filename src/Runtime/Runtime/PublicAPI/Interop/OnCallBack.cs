﻿
/*===================================================================================
* 
*   Copyright (c) Userware/OpenSilver.net
*      
*   This file is part of the OpenSilver Runtime (https://opensilver.net), which is
*   licensed under the MIT license: https://opensource.org/licenses/MIT
*   
*   As stated in the MIT license, "the above copyright notice and this permission
*   notice shall be included in all copies or substantial portions of the Software."
*  
\*====================================================================================*/

using CSHTML5.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;

#if BRIDGE
using Bridge;
using DotNetBrowser;
#endif

#if OPENSILVER
using Microsoft.JSInterop;
#endif

namespace CSHTML5.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class OnCallBack
    {
        static OnCallBack()
        {
            CheckIsRunningInBrowser();
        }

        [JSInvokable]
        public static void OnCallbackFromJavaScriptError(string idWhereCallbackArgsAreStored)
        {
            OnCallBackImpl.Instance.OnCallbackFromJavaScriptError(idWhereCallbackArgsAreStored);
        }

        // This method can be removed later. Now it is used for easier migration from old opensilver.js to new one
        [JSInvokable]
        public static object OnCallbackFromJavaScript(
            int callbackId,
            string idWhereCallbackArgsAreStored,
            object[] callbackArgsObject)
        {
            return OnCallbackFromJavaScriptBrowser(callbackId, idWhereCallbackArgsAreStored, callbackArgsObject,
                false);
        }

        [JSInvokable]
        public static object OnCallbackFromJavaScriptBrowser(
            int callbackId,
            string idWhereCallbackArgsAreStored,
            object[] callbackArgsObject,
            bool returnValue)
        {
            return OnCallBackImpl.Instance.OnCallbackFromJavaScript(callbackId, idWhereCallbackArgsAreStored, callbackArgsObject,
                MakeArgumentsForCallbackBrowser, false, returnValue);
        }

        private static object[] MakeArgumentsForCallbackBrowser(
            int count,
            int callbackId,
            string idWhereCallbackArgsAreStored,
            object[] callbackArgs,
            Type[] callbackGenericArgs)
        {
            var result = new object[count];

            for (int i = 0; i < count; i++)
            {
                var arg = new INTERNAL_JSObjectReference(callbackArgs, idWhereCallbackArgsAreStored, i);
                if (callbackGenericArgs != null
                    && i < callbackGenericArgs.Length
                    && callbackGenericArgs[i] != typeof(object)
                    && (
                    callbackGenericArgs[i].IsPrimitive
                    || callbackGenericArgs[i] == typeof(string)))
                {
                    // Attempt to cast from JS object to the desired primitive or string type. This is useful for example when passing an Action<string> to an Interop.ExecuteJavaScript so as to not get an exception that says that it cannot cast the JS object into string (when running in the Simulator only):
                    result[i] = Convert.ChangeType(arg, callbackGenericArgs[i]);
                }
                else
                {
                    result[i] = arg;
                }
            }
            return result;
        }

        private static void CheckIsRunningInBrowser()
        {
            if (OpenSilver.Interop.IsRunningInTheSimulator)
            {
                throw new InvalidOperationException($"'{nameof(OnCallBack)}' is not supported in the simulator.");
            }
        }
    }
}
