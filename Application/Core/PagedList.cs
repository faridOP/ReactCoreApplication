using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Application.Core
{

    public class PagingParams
    {
        private const int MaxPageSize = 50;
        public int PageNumber { get; set; } =1;
        private int _pageSize =10;
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = (value > MaxPageSize) ? MaxPageSize : value; }
        }

    }

    public class ActivityParams:PagingParams{
        public bool IsGoing { get; set; }
        public bool IsHost { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
    }

    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedList<T>(items, count, pageNumber, pageSize);
        }

        public PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
        {
            this.CurrentPage = pageNumber;
            this.PageSize = pageSize;
            this.TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            this.TotalCount = count;
            AddRange(items);
        }


    }



}