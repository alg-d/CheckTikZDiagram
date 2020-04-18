using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CheckTikZDiagram
{
    /// <summary>
    /// EditCollectionPanel.xaml の相互作用ロジック
    /// </summary>
    public partial class EditCollectionPanel : UserControl
    {
        public static readonly DependencyProperty ItemListProperty = DependencyProperty.Register(
            "ItemList",
            typeof(ObservableCollection<string>),
            typeof(EditCollectionPanel)
        );

        public ObservableCollection<string> ItemList
        {
            get { return (ObservableCollection<string>)GetValue(ItemListProperty); }
            set { SetValue(ItemListProperty, value); }
        }


        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
            "SelectedIndex",
            typeof(int),
            typeof(EditCollectionPanel),
            new PropertyMetadata(-1, (depObj, e) =>
            {
                var panel = (EditCollectionPanel)depObj;
                var index = (int)e.NewValue;
                if (index >= 0)
                {
                    panel.Value = panel.ItemList[index];
                }
                else
                {
                    panel.Value = "";
                }
            }));

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }


        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(string),
            typeof(EditCollectionPanel),
            new PropertyMetadata(""));

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }


        public EditCollectionPanel()
        {
            InitializeComponent();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            ItemList.RemoveAt(SelectedIndex);
            SelectedIndex = -1;
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemList[SelectedIndex] == Value)
            {
                SelectedIndex = -1;
            }
            else if (ItemList.Contains(Value))
            {
                MessageBox.Show(Value + "は既に登録されています");
            }
            else
            {
                ItemList[SelectedIndex] = Value;
                SelectedIndex = -1;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemList.Contains(Value))
            {
                MessageBox.Show(Value + "は既に登録されています");
            }
            else
            {
                ItemList.Add(Value);
                SelectedIndex = -1;
            }
        }
    }
}
