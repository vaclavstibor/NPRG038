using System.Reflection;
using System.Linq.Expressions;

namespace LinqToMinimalSql {

	class SqlTable<T> : IEnumerable<T> where T : new() {
		// Client side data:
		FieldInfo[] columnFields;

		internal readonly Dictionary<string, string> FieldNameToColumnNameMap = new Dictionary<string, string>();

		// DB server side data:
		string[] columnNames;
		string[][] data;

		public SqlTable(string[] columnNames, string[][] data) {
			this.columnNames = columnNames;
			this.data = data;

			var fieldInfos = typeof(T).GetFields();

			columnFields = new FieldInfo[columnNames.Length];
			for (int i = 0; i < columnFields.Length; i++) {
				columnFields[i] = FindFieldInfoForDatabaseColumn(columnNames[i], fieldInfos);
				FieldNameToColumnNameMap.Add(columnFields[i].Name, columnNames[i]);
			}
		}

		private static FieldInfo FindFieldInfoForDatabaseColumn(string columnName, FieldInfo[] fieldInfos) {
			columnName = new string(columnName.Where(c => c != '_').ToArray());

			foreach (var fi in fieldInfos) {
				if (StringComparer.InvariantCultureIgnoreCase.Compare(fi.Name, columnName) == 0) {
					return fi;
				}
			}

			return null;
		}

		public IEnumerator<T> GetEnumerator() {
			Console.WriteLine("  + SqlTable<{0}>.GetEnumerator() called ...", typeof(T).Name);

			foreach (var t in FilteredView(null)) {
				Console.WriteLine("  + SqlTable<{0}>.Enumerator.MoveNext() called, .Current <- {1} ...", typeof(T).Name, t);
				yield return t;
			}

			Console.WriteLine("  + SqlTable<{0}>.GetEnumerator() finished ...", typeof(T).Name);
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		//
		//
		//

	#region DB server query execution engine (simulates a remote database server)

		public class SqlWhereClause {
			public string ColumnName;
			public bool CompareOnEquality;
			public string Value;
		}

		private bool CheckRowSatisfiesWhere(string[] row, SqlWhereClause where) {
			int columnIdx = Array.IndexOf(columnNames, where.ColumnName);

			Console.WriteLine("DB server (i.e. SqlTable<T>) testing condition: WHERE {0} {1} {2} for {3}", where.ColumnName, where.CompareOnEquality ? "=" : "<>", where.Value, row[columnIdx]);

			return (row[columnIdx] == where.Value) ^ (!where.CompareOnEquality);
		}

		private IEnumerable<string[]> GetFilteredViewLazyResultSet(List<SqlWhereClause> whereClauses) {
			Console.WriteLine("DB server received a query and starting its execution ..."); 
			foreach (string[] row in data) {
				bool processRow = true;

				if (whereClauses != null) {
					foreach (var where in whereClauses) {
						processRow = CheckRowSatisfiesWhere(row, where);
						if (!processRow) break;
					}
				}

				if (processRow) {
					yield return row;
				}
			}
			Console.WriteLine("DB server completed query execution ..."); 
		}

	#endregion

		public IEnumerable<T> FilteredView(List<SqlWhereClause> whereClauses) {
			var lazyResultSet = GetFilteredViewLazyResultSet(whereClauses);

			foreach (string[] row in lazyResultSet) {				
				T t = new T();

				for (int i = 0; i < row.Length; i++) {
					columnFields[i].SetValue(t, row[i]);
				}

				yield return t;
			}
		}

		#region Actual LINQ to Minimal SQL implementation

		//
		// LINQ to Minimal SQL implementation begins here:
		//

	}

		#endregion

	class Person {
		public string FirstName;
		public string LastName;

		public override string ToString() {
 			return string.Format("Person(FirstName = {0}, LastName = {1})", FirstName, LastName);
		}
	}

	class Program {
		static void Main(string[] args) {
			string[] columns = { "LAST_NAME", "FIRST_NAME" };
			string[][] data = {
								  new [] { "Jezek", "Pavel" },
								  new [] { "Kofron", "Jan" },
								  new [] { "Jezek", "Ota" },
								  new [] { "Vesely", "Jiri" },
								  new [] { "Adamek", "Jiri" },
								  new [] { "Vesely", "Petr" },
								  new [] { "Humpolicek", "Jiri" }
							  };

			SqlTable<Person> persons = new SqlTable<Person>(columns, data);

			Console.WriteLine("Create query for LINQ to Objects ...");
			var q = from p in persons orderby p.LastName where p.FirstName == "Jiri" select p;

			Console.WriteLine("LINQ to Objects:");
			foreach (var p in q) {
				Console.WriteLine("\t{0}", p);
			}
			Console.WriteLine();

			//
			//
			//

			Console.WriteLine("Create query for FilteredView ...");
			var filter = new List<SqlTable<Person>.SqlWhereClause>();
			filter.Add(new SqlTable<Person>.SqlWhereClause { ColumnName = "FIRST_NAME", CompareOnEquality = true, Value = "Jiri" });
			filter.Add(new SqlTable<Person>.SqlWhereClause { ColumnName = "LAST_NAME", CompareOnEquality = false, Value = "Adamek" });

			Console.WriteLine("FilteredView:");
			foreach (var p in persons.FilteredView(filter)) {
				Console.WriteLine("\t{0}", p);
			}
			Console.WriteLine();

			//
			//
			//

			Console.WriteLine("Create query for LINQ to Minimal SQL ...");
			var q3 = from p in persons where p.FirstName == "Jiri" where p.LastName != "Adamek" select p;

			Console.WriteLine("LINQ to Minimal SQL:");
			foreach (var p in q3) {
				Console.WriteLine("\t{0}", p);
			}
			Console.WriteLine();
			
		}
	}
}
