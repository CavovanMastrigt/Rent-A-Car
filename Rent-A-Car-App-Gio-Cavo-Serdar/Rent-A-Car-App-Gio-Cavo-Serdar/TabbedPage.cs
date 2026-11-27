using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Rent_A_Car_App_Gio_Cavo_Serdar
{
    // Eén knop in de zijbalk met meerdere schermen als tabbladen (bijvoorbeeld Reserveringen met
    // Dagoverzicht, Planning, ...). Een tabblad maakt zijn scherm pas aan als je het opent,
    // zodat de app niet alle schermen tegelijk hoeft te laden.
    internal class TabbedPage : UserControl
    {
        // Eén tabblad: de titel, het recht dat erbij hoort en hoe het scherm wordt aangemaakt.
        public class Tab
        {
            public readonly string Text;
            public readonly string Perm;
            public readonly Func<UserControl> CreatePage;

            public Tab(string text, string perm, Func<UserControl> createPage)
            {
                Text = text;
                Perm = perm;
                CreatePage = createPage;
            }
        }

        private readonly TabControl tabs = new TabControl { Dock = DockStyle.Fill };
        private readonly List<Tab> definitions;
        private readonly UserControl[] pages;

        // selectPerm: het tabblad dat open moet gaan (bij doorverwijzen vanaf een ander scherm); null = het eerste.
        public TabbedPage(List<Tab> definitions, string selectPerm)
        {
            Ui.Style(this);
            BackColor = Ui.Light;
            Padding = new Padding(16);

            this.definitions = definitions;
            pages = new UserControl[definitions.Count];
            foreach (Tab tab in definitions)
            {
                tabs.TabPages.Add(new TabPage(tab.Text) { BackColor = Color.White });
            }
            Controls.Add(tabs);

            int start = selectPerm == null ? -1 : definitions.FindIndex(t => t.Perm == selectPerm);
            if (start > 0)
            {
                tabs.SelectedIndex = start;
            }
            Open(tabs.SelectedIndex);
            tabs.SelectedIndexChanged += (s, e) => Open(tabs.SelectedIndex);
        }

        // Het scherm van het tabblad dat nu open staat (of null als het niet geopend kon worden).
        public UserControl CurrentPage
        {
            get
            {
                int index = tabs.SelectedIndex;
                return index >= 0 && index < pages.Length ? pages[index] : null;
            }
        }

        // Opent het tabblad met dit recht.
        public void SelectByPerm(string perm)
        {
            int index = definitions.FindIndex(t => t.Perm == perm);
            if (index >= 0)
            {
                tabs.SelectedIndex = index;
                Open(index);
            }
        }

        // Maakt het scherm van een tabblad aan, één keer.
        private void Open(int index)
        {
            if (index < 0 || index >= pages.Length || pages[index] != null)
            {
                return;
            }

            try
            {
                UserControl page = definitions[index].CreatePage();
                page.Dock = DockStyle.Fill;
                tabs.TabPages[index].Controls.Add(page);
                pages[index] = page;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Deze pagina kon niet geopend worden:\n" + ex.Message,
                    "Pagina", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
