using System;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Small in-process hub so one admin page can send the user to another page
    // (and hand it an argument). The active AdminPage wires up Handler; child
    // pages and popups just call Open(). If the target page is hidden for this
    // user (no permission) the AdminPage simply ignores the request.
    public static class AdminNav
    {
        // perm = PermissionHelper key of the target page; arg = optional payload
        // (e.g. a user id or rental id) passed to the page via INavTarget.
        public static Action<string, object> Handler;

        public static void Open(string perm, object arg = null)
        {
            var h = Handler;
            if (h != null) h(perm, arg);
        }
    }

    // A page implements this to receive the argument after it is navigated to.
    public interface INavTarget
    {
        void ApplyNavArg(object arg);
    }
}
