﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#if !NETCORE
using System.Data.Entity;
#else
using Microsoft.EntityFrameworkCore;
#endif
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace Force.DeepCloner.Tests
{
#if !NETCORE
	[TestFixture(false)]
#endif
	[TestFixture(true)]
	public class SpecificScenariosTest : BaseTest
	{
		public SpecificScenariosTest(bool isSafeInit)
			: base(isSafeInit)
		{
		}

		[Test]
		public void Test_ExpressionTree_OrderBy1()
		{
			var q = Enumerable.Range(1, 5).Reverse().AsQueryable().OrderBy(x => x);
			var q2 = q.DeepClone();
			Assert.That(q2.ToArray()[0], Is.EqualTo(1));
			Assert.That(q.ToArray().Length, Is.EqualTo(5));
		}

		[Test]
		public void Test_ExpressionTree_OrderBy2()
		{
			var l = new List<int> { 2, 1, 3, 4, 5 }.Select(y => new Tuple<int, string>(y, y.ToString(CultureInfo.InvariantCulture)));
			var q = l.AsQueryable().OrderBy(x => x.Item1);
			var q2 = q.DeepClone();
			Assert.That(q2.ToArray()[0].Item1, Is.EqualTo(1));
			Assert.That(q.ToArray().Length, Is.EqualTo(5));
		}

		[Test(Description = "Tests works on local SQL Server with AdventureWorks database")]
		public void Clone_EfQuery1()
		{
			var q = new AdventureContext().Currencies.Where(x => x.CurrencyCode == "AUD");
			var q2 = q.DeepClone();
			Assert.That(q.ToArray().Length, Is.EqualTo(1));
			Assert.That(q2.ToArray().Length, Is.EqualTo(1));
		}

		[Test(Description = "Tests works on local SQL Server with AdventureWorks database")]
		public void Clone_EfQuery2()
		{
			var q = new AdventureContext().Currencies.OrderBy(x => x.Name);
			var q2 = q.DeepClone();
			 var cnt = q2.Count();
			Assert.That(q2.Count(), Is.EqualTo(cnt));
		}

		[Test]
		public void Clone_ComObject1()
		{
#if !NETCORE
// ReSharper disable SuspiciousTypeConversion.Global
			var manager = (KnownFolders.IKnownFolderManager)new KnownFolders.KnownFolderManager();
// ReSharper restore SuspiciousTypeConversion.Global
			Guid knownFolderID1;
			Guid knownFolderID2;
			manager.FolderIdFromCsidl(0, out knownFolderID1);
			manager.DeepClone().FolderIdFromCsidl(0, out knownFolderID2);
			Assert.That(knownFolderID1, Is.EqualTo(knownFolderID2));
#endif
		}

		[Test]
		public void Clone_ComObject2()
		{
#if !NETCORE
			Type t = Type.GetTypeFromProgID("SAPI.SpVoice");
			var obj = Activator.CreateInstance(t);
			obj.DeepClone();
#endif
		}

		[Table("Currency", Schema = "Sales")]
		public class Currency
		{
			[Key]
			public string CurrencyCode { get; set; }

			[Column]
			public string Name { get; set; }
		}

		public class AdventureContext : DbContext
		{
			public AdventureContext()
#if !NETCORE
			: base("Server=.;Integrated Security=SSPI;Database=AdventureWorks")
#endif
			{
			}

			public DbSet<Currency> Currencies { get; set; }

#if NETCORE
			protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
			{
				optionsBuilder.UseSqlServer(@"Server=.;Database=AdventureWorks;Trusted_Connection=true;MultipleActiveResultSets=true");
			}
#endif
		}

		[Test]
		public void GenericComparer_Clone()
		{
			var comparer = new TestComparer();
			comparer.DeepClone();
		}

		private class TestComparer : Comparer<int>
		{
			// make object unsafe to work
			private object _fieldX = new object();

			public override int Compare(int x, int y)
			{
				return x.CompareTo(y);
			}
		}

#if !NETCORE
		public class KnownFolders
		{
			[Guid("8BE2D872-86AA-4d47-B776-32CCA40C7018"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
			internal interface IKnownFolderManager
			{
				void FolderIdFromCsidl(int csidl, [Out] out Guid knownFolderID);

				void FolderIdToCsidl([In] [MarshalAs(UnmanagedType.LPStruct)] Guid id, [Out] out int csidl);

				void GetFolderIds();
			}

			[ComImport, Guid("4df0c730-df9d-4ae3-9153-aa6b82e9795a")]
			internal class KnownFolderManager
			{
				// make object unsafe to work
#pragma warning disable 169
				private object _fieldX;
#pragma warning restore 169
			}
		}
#endif
	}
}
