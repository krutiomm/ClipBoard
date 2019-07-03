using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace clipboard
{
    public class ClipBoardModel
    {
        public string Value { get; set; }
        public int Id { get; set; }
    }

    public class ClipBoardManager
    {
        public int Id;
        public ObservableCollection<ClipBoardModel> ClipBoardSource;
        private ClipBoardModel _lastModel;
        public Int16 recordCount = 10;
        /// <summary> 剪贴板内容改变 </summary>
        public void ClipboardChanged()
        {
            string text = Clipboard.GetText();

            if (!string.IsNullOrEmpty(text))
            {
                if (_lastModel != null && _lastModel.Value == text)
                {
                    return;
                }

                if (ClipBoardSource == null)
                {
                    Id++;
                    _lastModel = new ClipBoardModel() { Id = Id, Value = text };

                    ClipBoardSource = new ObservableCollection<ClipBoardModel>()
                        {_lastModel};

                }
                else
                {
                    Id++;
                    _lastModel = new ClipBoardModel() { Id = Id, Value = text };

//                    ClipBoardSource.Add(_lastModel);
                    ClipBoardSource.Insert(0, _lastModel);
                    //                    ClipBoardSource = new ObservableCollection<ClipBoardModel>
                    //                        (ClipBoardSource.OrderByDescending(item => item.Id));

                    MoveSource();

                }

            }
        }
        public void CopyToBoard(ClipBoardModel model)
        {

            ClipBoardSource.Remove(model);
            if (model != null)
            {
                _lastModel = null;
                if (!string.IsNullOrEmpty(model.Value)) Clipboard.SetText(model.Value);

            }
        }

        private void MoveSource()
        {
            int count = ClipBoardSource.Count;
            if (count > 10)
            {
                ClipBoardSource.Remove(ClipBoardSource.LastOrDefault());
            }
        }

    }
}
