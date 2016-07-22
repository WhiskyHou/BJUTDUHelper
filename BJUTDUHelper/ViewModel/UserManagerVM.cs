using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace BJUTDUHelper.ViewModel
{
    public class UserManagerVM:ViewModelBase
    {
        private readonly string messageToken = "1";
        public List<AccountType> AccountList { get; set; } =new List<AccountType>
        {
           new AccountType { Name="信息门户"},
           new AccountType { Name="教务系统"},
           new AccountType { Name="图书馆"}
        };
        public List<Model.ThemeColorModel> ThemeColors { get; set; } =new List<Model.ThemeColorModel>
        {   new Model.ThemeColorModel { Name="浪漫紫",ThemeColor=new Windows.UI.Color {A=255,  R=190,G=60,B=253 } },
            new Model.ThemeColorModel { Name="烟波蓝",ThemeColor=new Windows.UI.Color {A=255,  R=29,G=145,B=255 } },
            new Model.ThemeColorModel { Name="橄榄绿",ThemeColor=new Windows.UI.Color { A=255,R=50,G=180,B=52 } },
            new Model.ThemeColorModel { Name="落日黄",ThemeColor=new Windows.UI.Color { A=255, R=255,G=153,B=35 } },
            new Model.ThemeColorModel { Name="少女粉",ThemeColor=new Windows.UI.Color {A=255,  R=250,G=97,B=152 } }
        };
        public class AccountType
        {
            public string Name { get; set; }
            public Model.UserBase UserInfo{get;set;}
        }

        public UserManagerVM()
        {
            SaveCommand = new RelayCommand<object>(Save);
            //ClearCommand = new RelayCommand<object>(Clear);
            LoadedCommand = new RelayCommand(Loaded);
        }
        public ICommand LoadedCommand { get; set; }
      
        public void Loaded()
        {
            LoadAccountInfo();//加载本地账号信息
        }
        public async void ThemeItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as Model.ThemeColorModel;
            Service.SettingService.SetThemeColor(item.ThemeColor);
            
            //存储颜色
            DAL.LocalSetting _localSetting = new DAL.LocalSetting();
            await _localSetting.SetLocalInfo<Model.ThemeColorModel>(typeof(Model.ThemeColorModel).Name, item);
        }
        public ICommand SaveCommand { get; set; }
        public void Save(object selItem)
        {
            var accountType = selItem as AccountType;
            try
            {
                Manager.AccountManager.SetAccount(accountType.UserInfo);
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send<string>("保存成功", messageToken);
            }
            catch(NullReferenceException nullRef)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send<string>(nullRef.Message, messageToken);
            }
            catch
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send<string>("保存失败", messageToken);
            }
           
        }
       
        private async void LoadAccountInfo()
        {
            var infocenter= await Manager.AccountManager.GetAccount<Model.BJUTInfoCenterUserinfo>();
            var educenter = await Manager.AccountManager.GetAccount<Model.BJUTEduCenterUserinfo>();
            var libcenter = await Manager.AccountManager.GetAccount<Model.BJUTLibCenterUserinfo>();

            AccountList[0].UserInfo = infocenter == null ? new Model.BJUTInfoCenterUserinfo() : infocenter;
            AccountList[1].UserInfo = educenter == null ? new Model.BJUTEduCenterUserinfo() : educenter;
            AccountList[2].UserInfo = libcenter == null ? new Model.BJUTLibCenterUserinfo() : libcenter;

        }
    }
}
