using System;
using System.Windows;

namespace AllocationRerouter
{
    public static class PropertyChangedNotificationInterceptor
    {
#pragma warning disable IDE0060 // Remove unused parameter
        public static void Intercept(object target, Action onPropertyChangedAction, string proprtyName)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            Application.Current.Dispatcher.Invoke(onPropertyChangedAction);
        }
    }
}
