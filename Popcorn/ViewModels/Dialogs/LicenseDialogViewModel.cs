using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using NLog;

namespace Popcorn.ViewModels.Dialogs
{
    public class LicenseDialogViewModel : ViewModelBase
    {
        /// <summary>
        /// Logger of the class
        /// </summary>
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Close dialog
        /// </summary>
        public Action OnCloseAction { get; set; }

        /// <summary>
        /// <see cref="CloseCommand"/>
        /// </summary>
        private ICommand _closeCommand;

        /// <summary>
        /// License
        /// </summary>
        private string _license;

        /// <summary>
        /// Constructor
        /// </summary>
        public LicenseDialogViewModel()
        {
            var subjectType = GetType();
            var subjectAssembly = subjectType.Assembly;
            using (var stream = subjectAssembly.GetManifestResourceStream(@"Popcorn.Markdown.License.md"))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        License = reader.ReadToEnd();
                    }
                }
            }

            CloseCommand = new RelayCommand(() =>
            {
                OnCloseAction.Invoke();
            });
        }

        /// <summary>
        /// The license
        /// </summary>
        public string License
        {
            get => _license;
            set { Set(() => License, ref _license, value); }
        }

        public ICommand CloseCommand
        {
            get => _closeCommand;
            set => Set(ref _closeCommand, value);
        }
    }
}
