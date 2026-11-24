namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    partial class ProfileForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (picPhoto != null && picPhoto.Image != null) picPhoto.Image.Dispose();
                if (components != null) components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
