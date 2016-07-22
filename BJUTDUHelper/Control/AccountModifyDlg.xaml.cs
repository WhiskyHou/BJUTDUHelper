using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace BJUTDUHelper.Control
{
    public sealed partial class AccountModifyDlg : UserControl
    {
        public AccountModifyDlg()
        {
            this.InitializeComponent();
            //this.Visibility = Visibility.Collapsed;
            this.btnClose.Click += (o, e) => { this.CloseDlgStoryboard.Begin(); Open = false; };
            this.btnSave.Click += (o, e) => { this.CloseDlgStoryboard.Begin(); Open = false;Saved?.Invoke(User); };
            User = new Model.UserBase();
        }
        public Model.UserBase User
        {
            get { return (Model.UserBase)this.GetValue(UserProperty); }
            set { SetValue(UserProperty, value); }
        }
        public static readonly DependencyProperty UserProperty = DependencyProperty.Register("User", typeof(Model.UserBase), typeof(AccountModifyDlg), new PropertyMetadata(null));

        public ICommand SaveCommand
        {
            get { return (ICommand)GetValue(SaveCommandProperty); }
            set { SetValue(SaveCommandProperty, value); }
        }
        public static readonly DependencyProperty SaveCommandProperty = DependencyProperty.Register("SaveCommand", typeof(ICommand), typeof(AccountModifyDlg), new PropertyMetadata(null,
            (o,e)=> 
            {
                AccountModifyDlg dlg = o as AccountModifyDlg;
                dlg.btnSave.Click += (sender, args) => { dlg.SaveCommand?.Execute(dlg.User); };
            }));

        public static readonly DependencyProperty OpenProperty = DependencyProperty.Register("Open", typeof(bool), typeof(AccountModifyDlg), new PropertyMetadata(false, (o, e) =>
        {
            var dlg = o as AccountModifyDlg;

            var isopen = dlg.Open;
            if (isopen == true)
            {
                dlg.OpenDlgStoryboard.Begin();

            }
        }));
        
        public bool Open
        {
            get { return (bool)GetValue(OpenProperty); }
            set { SetValue(OpenProperty, value); }
        }


        //public Action Saved
        //{
        //    get { return (Action)GetValue(SavedProperty); }
        //    set { SetValue(SavedProperty, value); }
        //}
        //public static readonly DependencyProperty SavedProperty = DependencyProperty.Register("Saved", typeof(Action), typeof(AccountModifyDlg), new PropertyMetadata(null));
        public Action<object> Saved { get; set; }
    }
}
