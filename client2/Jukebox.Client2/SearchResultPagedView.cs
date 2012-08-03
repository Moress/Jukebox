using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using Jukebox.Client2.JukeboxService;

namespace Jukebox.Client2
{
    public class SearchResultPagedView : IEnumerable, IPagedCollectionView, INotifyPropertyChanged, INotifyCollectionChanged
    {
        private bool isPageChanging;

        private int pageSize;

        private int GetPageCount()
        {
            var result =  ItemCount/pageSize;
            if (result * pageSize < ItemCount)
            {
                result++;
            }

            return result;
        }


        private readonly SearchServiceClient searchServiceClient;
        ObservableCollection<Track> foundTracks = new ObservableCollection<Track>();

        public SearchResultPagedView()
        {
            searchServiceClient = new SearchServiceClient();
            searchServiceClient.SearchCompleted += (sender, ea) =>
                                             {
                                                 ItemCount = ea.Result.TotalCount;
                                                 TotalItemCount = ea.Result.TotalCount;
                                                 foundTracks = ea.Result.FoundTracks;
                                                 NotBusy();
                                                 OnCollectionChanged();
                                             };

            PageChanged += (sender, ea) => CallSearch();
        }

        public void CallSearch()
        {
            Busy();
            searchServiceClient.SearchAsync(query, sources, PageIndex, PageSize);
        }

        public IEnumerator GetEnumerator()
        {
            return foundTracks.GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<EventArgs> PageChanged;

        public event EventHandler<PageChangingEventArgs> PageChanging;

        private string query;

        public string Query 
        {
            get
            {
                return query;
            }
            set
            {
                query = value;
            }
        }


        private BusyIndicator busyIndicator;

        public BusyIndicator BusyIndicator
        {
            get
            {
                return busyIndicator;
            }
            set
            {
                busyIndicator = value;
            }
        }

        private void Busy()
        {
            if (busyIndicator != null)
            {
                busyIndicator.IsBusy = true;
            }
        }
        private void NotBusy()
        {
            if (busyIndicator != null)
            {
                busyIndicator.IsBusy = false;
            }
        }

        private ObservableCollection<TrackSourceComboItem> sources;

        public ObservableCollection<TrackSourceComboItem> Sources
        {
            get
            {
                return sources;
            }
            set
            {
                sources = value;
            }
        }
        
        bool IPagedCollectionView.CanChangePage
        {
            get { return true; }
        }

        bool IPagedCollectionView.IsPageChanging
        {
            get { return isPageChanging; }
        }


        private int itemCount;
        public int ItemCount
        {
            get
            {
                return itemCount;
            }
            set
            {
                itemCount = value;
                OnPropertyChanged("ItemCount");
            }
        }


        bool IPagedCollectionView.MoveToFirstPage()
        {
            return ((IPagedCollectionView) this).MoveToPage(0);
        }

        bool IPagedCollectionView.MoveToLastPage()
        {
            return ((IPagedCollectionView)this).MoveToPage(GetPageCount() - 1);
        }


        bool IPagedCollectionView.MoveToNextPage()
        {
            return ((IPagedCollectionView) this).MoveToPage(((IPagedCollectionView) this).PageIndex + 1);
        }

        bool IPagedCollectionView.MoveToPage(int newIndex)
        {
            if (OnPageChanging(newIndex) && newIndex != -1)
           { 
                return false;
            }

            if (newIndex >= GetPageCount())
            {
                return false;
            }


            SetIsPageChanging(true);

            this.PageIndex = newIndex;

            SetIsPageChanging(false);


            OnPropertyChanged("PageIndex");

            OnPropertyChanged("ItemCount");

            OnPageChanged();

            OnCollectionChanged();


            return true;
        }


        bool IPagedCollectionView.MoveToPreviousPage()
        {
            return ((IPagedCollectionView) this).MoveToPage(((IPagedCollectionView) this).PageIndex - 1);
        }

        public int PageIndex { get; set; }

        public int PageSize
        {
            get { return pageSize; }

            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("value",
                                                          "The PageSize of an endless collection should be positive.");
                }


                if (pageSize != value)
                {
                    pageSize = value;

                    OnPropertyChanged("PageSize");

                    OnPropertyChanged("ItemCount");

                    ((IPagedCollectionView) this).MoveToFirstPage();
                }
            }
        }

        private int totalItemCount;
        public int TotalItemCount
        {
            get
            {
                return totalItemCount;
            }
            set
            {
                totalItemCount = value;
                OnPropertyChanged("TotalItemCount");
            }
        }

        private void SetIsPageChanging(bool value)
        {
            if (isPageChanging != value)
            {
                isPageChanging = value;

                OnPropertyChanged("IsPageChanging");
            }
        }


        private void OnPropertyChanged(string propertyName)
        {
            var e = new PropertyChangedEventArgs(propertyName);

            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }


        private bool OnPageChanging(int newPageIndex)
        {
            var e = new PageChangingEventArgs(newPageIndex);

            if (PageChanging != null)
            {
                PageChanging(this, e);
            }


            return e.Cancel;
        }


        private void OnPageChanged()
        {
            EventArgs e = EventArgs.Empty;

            if (PageChanged != null)
            {
                PageChanged(this, e);
            }
        }


        private void OnCollectionChanged()
        {
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

            if (CollectionChanged != null)
            {
                CollectionChanged(this, e);
            }
        }
    }
}
