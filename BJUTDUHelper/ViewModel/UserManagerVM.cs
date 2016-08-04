using BJUTDUHelper.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public UserManagerVM()
        {
            SaveCommand = new RelayCommand<object>(Save);
            //ClearCommand = new RelayCommand<object>(Clear);
            LoadedCommand = new RelayCommand(Loaded);

            BJUTEduCenterUserinfos = new ObservableCollection<BJUTEduCenterUserinfo>();
            BJUTInfoCenterUserinfos = new ObservableCollection<BJUTInfoCenterUserinfo>();
            BJUTLibCenterUserinfos = new ObservableCollection<BJUTLibCenterUserinfo>();
        }

        public ICommand LoadedCommand { get; set; }
        public void Loaded()
        {
            LoadAccountInfo();//加载本地账号信息
        }

        #region 用户主题设置
        public List<Model.ThemeColorModel> ThemeColors { get; set; } =new List<Model.ThemeColorModel>
        {   new Model.ThemeColorModel { Name="浪漫紫",ThemeColor=new Windows.UI.Color {A=255,  R=190,G=60,B=253 } },
            new Model.ThemeColorModel { Name="烟波蓝",ThemeColor=new Windows.UI.Color {A=255,  R=29,G=145,B=255 } },
            new Model.ThemeColorModel { Name="橄榄绿",ThemeColor=new Windows.UI.Color { A=255,R=50,G=180,B=52 } },
            new Model.ThemeColorModel { Name="落日黄",ThemeColor=new Windows.UI.Color { A=255, R=255,G=153,B=35 } },
            new Model.ThemeColorModel { Name="少女粉",ThemeColor=new Windows.UI.Color {A=255,  R=250,G=97,B=152 } }
        };
        public async void ThemeItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as Model.ThemeColorModel;
            Service.SettingService.SetThemeColor(item.ThemeColor);

            //存储颜色
            DAL.LocalSetting _localSetting = new DAL.LocalSetting();
            await _localSetting.SetLocalInfo<Model.ThemeColorModel>(typeof(Model.ThemeColorModel).Name, item);
        }
        #endregion


        #region 账号管理
        private string _studentID;
        public string StudentID
        {
            get { return _studentID; }
            set { Set(ref _studentID, value); }
        }


        public ObservableCollection<Model.BJUTInfoCenterUserinfo> BJUTInfoCenterUserinfos { get; set; }
        public ObservableCollection<Model.BJUTEduCenterUserinfo>  BJUTEduCenterUserinfos { get; set; }
        public ObservableCollection<Model.BJUTLibCenterUserinfo> BJUTLibCenterUserinfos { get; set; }

        private BJUTInfoCenterUserinfo _infoUser = new BJUTInfoCenterUserinfo();
        public BJUTInfoCenterUserinfo InfoUser
        {
            get { return _infoUser; }
            set { Set(ref _infoUser, value); }
        }
        private BJUTEduCenterUserinfo _eduUser = new BJUTEduCenterUserinfo();
        public BJUTEduCenterUserinfo EduUser
        {
            get { return _eduUser; }
            set { Set(ref _eduUser, value); }
        }
        private BJUTLibCenterUserinfo _libUser = new BJUTLibCenterUserinfo();
        public BJUTLibCenterUserinfo LibUser
        {
            get { return _libUser; }
            set { Set(ref _libUser, value); }
        }

        public ICommand SaveCommand { get; set; }
        public void Save(object param)
        {
            string type = (string)param;
            switch (type)
            {
                case "BJUTInfoCenterUserinfo":
                    if (InfoUser == null || string.IsNullOrWhiteSpace(InfoUser.Password) || string.IsNullOrWhiteSpace(InfoUser.Username))
                        return;
                    Service.DbService.SaveInfoCenterUserinfo(InfoUser);
                    break;
                case "BJUTLibCenterUserinfo":
                    if (LibUser == null || string.IsNullOrWhiteSpace(LibUser.Password) || string.IsNullOrWhiteSpace(LibUser.Username))
                        return;
                    Service.DbService.SaveInfoCenterUserinfo(LibUser); break;
                case "BJUTEduCenterUserinfo":
                    if (EduUser == null || string.IsNullOrWhiteSpace(EduUser.Password) || string.IsNullOrWhiteSpace(EduUser.Username))
                        return;
                    Service.DbService.SaveInfoCenterUserinfo(EduUser); break;
                case "StudentID":
                    Service.FileService.SetStudentID(StudentID); break;
                default:
                    break;
            }

        }
        private async void LoadAccountInfo()
        {
            StudentID = Service.FileService.GetStudentID();

            var infouser=Service.DbService.GetInfoCenterUserinfo<Model.BJUTInfoCenterUserinfo>();
            foreach (var item in infouser)
            {
                BJUTInfoCenterUserinfos.Add(item);
            }

            var eduuser = Service.DbService.GetInfoCenterUserinfo<Model.BJUTEduCenterUserinfo>();
            foreach (var item in eduuser)
            {
                BJUTEduCenterUserinfos.Add(item);
            }

            var libuser = Service.DbService.GetInfoCenterUserinfo<Model.BJUTLibCenterUserinfo>();
            foreach (var item in libuser)
            {
                BJUTLibCenterUserinfos.Add(item);
            }
        }

        public void InfoUser_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            BJUTInfoCenterUserinfo user = args.SelectedItem as BJUTInfoCenterUserinfo;
            InfoUser = user;
        }
        public void EduUser_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            BJUTEduCenterUserinfo user = args.SelectedItem as BJUTEduCenterUserinfo;
            EduUser = user;

        }
        public void LibUser_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            BJUTLibCenterUserinfo user = args.SelectedItem as BJUTLibCenterUserinfo;
            LibUser = user;
        }
        #endregion

    }
}
