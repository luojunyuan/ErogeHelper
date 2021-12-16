using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;

namespace ErogeHelper.ShellMenuHandler
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".exe")]
    public class ShellMenuExtension : SharpContextMenu
    {
        private ContextMenuStrip _menu = new ContextMenuStrip();

        /// <summary>
        /// Determines whether the menu item can be shown for the selected item.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if item can be shown for the selected item for this instance.; 
        ///   otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanShowMenu()
        {
            // We can show the item only for a single selection.
            if (SelectedItemPaths.Count() != 1)
                return false;

            UpdateMenu();
            return true;
        }

        /// <summary>
        /// Creates the context menu. This can be a single menu item or a tree of them.
        /// Here we create the menu based on the type of item
        /// </summary>
        /// <returns>
        /// The context menu for the shell context menu.
        /// </returns>
        protected override ContextMenuStrip CreateMenu()
        {
            _menu.Items.Clear();

            var peType = PeFileReader.GetPeType(SelectedItemPaths.First());
            // check if the selected executable is 64 bit
            if (peType == PeType.X64)
            {
                MenuX64();
            }
            else if (peType == PeType.X32)
            {
                MenuX86();
            }

            // return the menu item
            return _menu;
        }

        private static readonly bool Is4K = SystemHelper.Is4KDisplay();

        private static readonly Image E = Is4K ? EmbeddedImage("E@200.png") : EmbeddedImage("E.bmp");

        private static readonly Image CheckboxComposite =
            Is4K ? EmbeddedImage("CheckboxComposite@200.png") : EmbeddedImage("CheckboxComposite14.png");
        private static readonly Image Checkbox =
            Is4K ? EmbeddedImage("Checkbox@200.png") : EmbeddedImage("Checkbox14.png");

        private void MenuX64()
        {
            var directStartItem = new ToolStripMenuItem
            {
                Text = Language.Strings.ShellMenu_DirectStart,
                Image = E
            };
            directStartItem.Click += (sender, args) => MainProcess(false, false);

            var adminStartItem = new ToolStripMenuItem
            {
                Text = Language.Strings.ShellMenu_RunAdministrator,
                Image = E
            };
            adminStartItem.Click += (sender, args) => MainProcess(false, true);

            _menu.Items.Clear();
            _menu.Items.Add(directStartItem);
            _menu.Items.Add(adminStartItem);
        }

        private const string AppName = "Eroge Helper";

        /// <summary>
        /// Creates the context menu when the selected .exe is 32 bit.
        /// </summary>
        protected void MenuX86()
        {
            var mainMenu = new ToolStripMenuItem
            {
                Text = AppName,
                Image = E
            };

            var directStartItem = new ToolStripMenuItem
            {
                Text = Language.Strings.ShellMenu_DirectStart,
                Image = E
            };

            var leStartItem = new ToolStripMenuItem
            {
                Text = Language.Strings.ShellMenu_LEStart,
                Image = E
            };

            directStartItem.Click += (sender, args) => MainProcess(false, _isAdmin);
            leStartItem.Click += (sender, args) => MainProcess(true, _isAdmin);

            mainMenu.DropDownItems.Add(directStartItem);
            mainMenu.DropDownItems.Add(leStartItem);
            var adminItem = new ToolStripMenuItem
            {
                Text = Language.Strings.ShellMenu_Administrator,
                Image = _isAdmin ? CheckboxComposite : Checkbox,
            };

            adminItem.Click += (sender, args) =>
            {
                _isAdmin = !_isAdmin;
            };
            mainMenu.DropDownItems.Add(adminItem);

            _menu.Items.Clear();
            _menu.Items.Add(mainMenu);
        }

        private static bool _isAdmin;

        private void MainProcess(bool useLe, bool isAdmin)
        {
            var startInfo = new ProcessStartInfo();

            // Get Path of dll (Same as project binary Path)
            // https://stackoverflow.com/questions/52797/how-do-i-get-the-path-of-the-assembly-the-code-is-in
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            string shellMenuDllPath;
            if (codeBase.StartsWith("file:///"))
            {
                var uri = new UriBuilder(codeBase);
                shellMenuDllPath = Uri.UnescapeDataString(uri.Path);
            }
            // EH in Shared Folder
            else
            {
                shellMenuDllPath = codeBase.Replace("file:", string.Empty);
            }
            var erogeHelperProcPath = Path.GetDirectoryName(shellMenuDllPath) + @"\ErogeHelper.exe";
            var gamePath = SelectedItemPaths.First();
            startInfo.FileName = erogeHelperProcPath;
            startInfo.Arguments = $"\"{gamePath}\"";
            startInfo.UseShellExecute = false;

            if (useLe)
            {
                // gamePath must be Args[0]
                startInfo.Arguments += " /le";
            }

            if (isAdmin)
            {
                //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.UseShellExecute = true;
                startInfo.Verb = "runas";
            }

            try
            {
                Process.Start(startInfo);
            }
            catch (Win32Exception e) when (e.NativeErrorCode == 1223) // ERROR_CANCELLED 
            {
                // The operation was canceled by the user.
            }
            catch (Win32Exception e) when (e.NativeErrorCode == 2) // AccessDenied
            {
                MessageBox.Show($@"Please make sure ""{erogeHelperProcPath}"" exists", "ErogeHelper");
            }
            catch (Win32Exception e)
            {
                MessageBox.Show(
                    $"{e} \n" +
                    $"ErrorCode: {e.ErrorCode}\n" +
                    $"NativeErrorCode: {e.NativeErrorCode}\n" +
                    $"ErogeHelper path: {erogeHelperProcPath}\n" +
                    $"Codebase: {codeBase}", "ErogeHelper");
            }
        }

        /// <summary>
        /// Updates the context menu. 
        /// </summary>
        private void UpdateMenu()
        {
            // release all resources associated to existing menu
            _menu.Dispose();
            _menu = CreateMenu();
        }

        private static Bitmap EmbeddedImage(string filename) =>
            new Bitmap(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "ErogeHelper.ShellMenuHandler.Resources." + filename) ?? throw new InvalidOperationException());
    }
}
