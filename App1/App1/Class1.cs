using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace App1
{
    public class Grouping<K, T> : ObservableCollection<T>
    {
        public K Name { get; private set; }
        public Grouping(K name, IEnumerable<T> items)
        {
            Name = name;
            foreach (T item in items)
                Items.Add(item);
        }
    }
    public class Phone
    {
        public string Title { get; set; }
        public string Company { get; set; }
        public int Price { get; set; }
    }
    class Class1 : INotifyPropertyChanged
    {
        public List<string> Mounth
        {
            get
            {
                return Mounth;
            }
            set
            {
                Mounth = value;
            }
        }
        public List<string> Years
        {
            get
            {
                return Years;
            }
            set
            {
                Years = value;
            }
        }
        public Class1()
        {
            Years = new List<string>();
            Mounth = new List<string>();
            var phones = new List<Phone>
            {
                new Phone {Title="Galaxy S8", Company="Samsung", Price=60000 },
                new Phone {Title="Galaxy S7 Edge", Company="Samsung", Price=50000 },
                new Phone {Title="Huawei P10", Company="Huawei", Price=10000 },
                new Phone {Title="Huawe Mate 8", Company="Huawei", Price=29000 },
                new Phone {Title="Mi6", Company="Xiaomi", Price=55000 },
                new Phone {Title="iPhone 7", Company="Apple", Price=38000 },
                new Phone {Title="iPhone 6S", Company="Apple", Price=50000 }
            };
            var groups = phones.GroupBy(p => p.Company).Select(g => new Grouping<string, Phone>(g.Key, g));
            // передаем группы в PhoneGroups
            PhoneGroups = new ObservableCollection<Grouping<string, Phone>>(groups);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<Grouping<string, Phone>> PhoneGroups { get; set; }

    }
}
