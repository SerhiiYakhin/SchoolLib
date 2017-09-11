﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using SchoolLib.Data;
using SchoolLib.Models.Books;
using SchoolLib.Models.People;
using SchoolLib.Models.StatisticViewModels;
using SchoolLib.Data.Extensions;

namespace SchoolLib.Controllers
{
    public class StatisticController : Controller
    {
        readonly ApplicationDbContext _context;

        public StatisticController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> Index() => View();
        public async Task<IActionResult> BookSearch(
            string startDate,
            string endDate,
            double? startPrice,
            double? endPrice,
            short? startPubDate,
            short? endPubDate,
            byte? startGrade,
            byte? endGrade,
            string actn
            )
        {
            var bookData = new BooksStatisticViewModel();
            IQueryable<Issuance> query = _context.Issuances.Include(i => i.Book);
            DateTime _start, _end;
            if (startDate.TryParse(out _start) && endDate.TryParse(out _end))
            {
                query = actn == "iss_books"
                    ? query.Where(i => i.IssueDate.Parse() >= _start
                            && i.IssueDate.Parse() <= _end)
                    : query.Where(i => i.IssueDate != null
                            && i.IssueDate.Parse() >= _start
                            && i.IssueDate.Parse() <= _end)
                    ;
            }
            priceCheck(startPrice, endPrice, ref query);
            yearCheck(startPubDate, endPubDate, ref query);
            bookData.AdBooks = new List<AdditionalBook>();
            bookData.StBooks = new List<StudyBook>();
            foreach (var iss in (await query.ToListAsync()))
            {
                if(iss.Book.Discriminator == "AdditionalBook")
                    bookData.AdBooks.Add((AdditionalBook)iss.Book);
                else if (((StudyBook)iss.Book).Grade >= startGrade
                        && ((StudyBook)iss.Book).Grade <= endGrade)
                    bookData.StBooks.Add((StudyBook)iss.Book);
            }
            return PartialView("_Books", bookData);
        }

        public async Task<IActionResult> ReaderSearch(
            string startDate,
            string endDate,
            byte? startGrade,
            byte? endGrade,
            string actn
            )
        {
            var bookData = new BooksStatisticViewModel();
            IQueryable<Issuance> query = _context.Issuances.Include(i => i.Book);
            DateTime _start, _end;
            if (startDate.TryParse(out _start) && endDate.TryParse(out _end))
            {
                query = actn == "iss_books"
                    ? query.Where(i => i.IssueDate.Parse() >= _start
                            && i.IssueDate.Parse() <= _end)
                    : query.Where(i => i.IssueDate != null
                            && i.IssueDate.Parse() >= _start
                            && i.IssueDate.Parse() <= _end)
                    ;
            }
            bookData.AdBooks = new List<AdditionalBook>();
            bookData.StBooks = new List<StudyBook>();
            foreach (var iss in (await query.ToListAsync()))
            {
                if (iss.Book.Discriminator == "AdditionalBook")
                    bookData.AdBooks.Add((AdditionalBook)iss.Book);
                else 
                    bookData.StBooks.Add((StudyBook)iss.Book);
            }
            return PartialView("_Books", bookData);
        }

        public static void priceCheck(double? start, double? end, ref IQueryable<Issuance> query)
        {
            if (start.HasValue && end.HasValue)
                query = query
                    .Where
                    (i => Double.Parse(i.Book.Price) >= start
                        && Double.Parse(i.Book.Price) <= end);
        }
        public static void yearCheck(short? start, short? end, ref IQueryable<Issuance> query)
        {
            if (start.HasValue && end.HasValue)
                query = query
                    .Where
                    (i => i.Book.Published >= start
                        && i.Book.Published <= end);
        }
    }
}
