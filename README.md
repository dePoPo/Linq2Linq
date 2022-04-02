# Unfinished work in progress

# Linq2Linq
Convert Linq2Sql to DevArt LinqConnect for a large codebase

This is not for generic use, this is offered to you as an example of how you could approach automatic conversion vs. manual updating.

# Background
Having a 5M+ lines codebase with Linq2Sql makes it hard to move. That codebase having a 15 year historie as a production project in a changing environment starting out as VB6, moving to VB.NET and since the last 10 years C# does not make it a lot easyer. And at some point converting that huge bunch of webforms and winforms to .net core (or 'just' .net as the name of the day is)  will be a major challenge.

Now of course there are multiple projects, and some already are .net while the vast majority is still .framework. To make conversion easyer it is a huge win if we can use the same all important data layer for both types of projects, as it will be much easyer to convert from framework to .net later on.

DevArt Linq2Sql offers the option to create more or less linq2sql compatible drop in code, with the major advantage of being able to create the exact same models and context classes for both modern .net and classic framework. 

So my requirement is simple:

i want to drop a file into my converter, which:

- Converts my linq2sql code to LinqConnect code
- Clean up some naming conventions (*1)
- Clean up some general notation and casting issues 

(*1) As any old enough database is a pig's breakfast and sqlmetal just goes along with whatever is in your database  l2sql is full of some_table_field_name with random capitalisation  where linqconnect can use SomeTableFieldName and normalize the database a bit.

To make that reality the general approach is:

- Scan the code file and create blocks form it. A block will be a method or function, etc.
- Scan the blocks for DataContext creation, and get the name of the variable holding it (var x = new DataContext)
- Change that context to the new DevArt LinqConnect context (var x = new LcDataContext)
- Using that variable, scan and update table names that are access (x.some_table_Name  => x.SomeTableName)
- Get the list of fields from the sql database for said table
- Convert field access  (.some_field => .SomeField)
- Cast as many multi record selections to .ToList() as linq2sql is a lot more MARS friendly then LinqConnect

Having run that, it would still be a good idea to scan the resulting code for other leftovers or obvious optimalisations, but especially the field name conversions can make a massive difference in the amount of manual work that is to be done.

As said at the top, this is not a ready made converter for all cases, but who knows - it may give you an idea or 2

# Example

# Before conversion
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asterix.Framework.Data.LcDomain;
using Asterix.Framework.Data.Interfaces;
using Asterix.Framework.Data.Models.Debiteuren;
using Asterix.Framework.Definitions;
using Asterix.Framework.Foundation;

namespace Asterix.Framework.Data.Repository.Debiteuren
{
    public class ContactpersoonRepository : IAsterixRepository<Contactpersoon>
    {
        private readonly string _database;

        public ContactpersoonRepository(RuntimeParameters parms) {
            _database = parms.Database;
        }

        public Contactpersoon Add(Contactpersoon model) {
            var dc = new LcAsterixDataContext(_database);
            var newCard = new Asterix_klanten_Contactpersonen();
            newCard.Cp_klantzoekcode = model.Klantnummer;
            newCard.Cp_achternaam = model.Achternaam;
            newCard.Cp_voorletters = model.Voorletters;
            newCard.Cp_voornaam = model.Voornaam;
            newCard.Cp_email = model.Email;
            newCard.Cp_functie = model.Functie;
            newCard.Cp_telefoon1 = model.Telefoon;
            newCard.Cp_telefoon2 = model.Telefoon2;
            newCard.Cp_geslacht = model.Geslacht;
            newCard.Cp_ishoofdcontact = model.IsHoofdcontact;
            ObjectFactory.SetDefaultFieldValues(newCard);
            dc.Asterix_klanten_Contactpersonen.InsertOnSubmit(newCard);
            dc.SubmitChanges();
            model.Id = newCard.Id;
            return model;
        }

        public void Delete(int id) {
            var dc = new LcAsterixDataContext(_database);
            var del = dc.Asterix_klanten_Contactpersonen.Single(x => x.Id == id);
            dc.Asterix_klanten_Contactpersonen.DeleteOnSubmit(del);
            dc.SubmitChanges();
        }

        public List<Contactpersoon> FullTextSearch(string filter) {
            filter = filter.Trim().ToLower();
            var filterDate = Helpers.GetFilterDate(filter);
            var dc = new LcAsterixDataContext(_database);
            var ds = dc.Asterix_klanten_Contactpersonen.Where(x => x.Id.ToString() == filter
                                                                   || x.Cp_klantzoekcode.ToLower().IndexOf(filter) != -1
                                                                   || x.Cp_achternaam.ToLower().IndexOf(filter) != -1
                                                                   || x.Cp_voorletters.ToLower().IndexOf(filter) != -1
                                                                   || x.Cp_voornaam.ToLower().IndexOf(filter) != -1
                                                                   || x.Cp_email.ToLower().IndexOf(filter) != -1
                                                                   || x.Cp_functie.ToLower().IndexOf(filter) != -1
                                                                   || x.Cp_telefoon1.ToLower().IndexOf(filter) != -1
                                                                   || x.Cp_telefoon2.ToLower().IndexOf(filter) != -1
                                                                   || x.Cp_geslacht.ToLower().IndexOf(filter) != -1
            );
            var resultSet = new List<Contactpersoon>();
            Parallel.ForEach(ds, card => {
                lock (resultSet) {
                    resultSet.Add(GetModel(card));
                }
            });
            return resultSet;
        }



#After conversion


using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asterix.Framework.Data.LcDomain;
using Asterix.Framework.Data.Interfaces;
using Asterix.Framework.Data.Models.Debiteuren;
using Asterix.Framework.Definitions;
using Asterix.Framework.Foundation;

namespace Asterix.Framework.Data.Repository.Debiteuren
{
    public class ContactpersoonRepository : IAsterixRepository<Contactpersoon>
    {
        private readonly string _database;

        public ContactpersoonRepository(RuntimeParameters parms) {
            _database = parms.Database;
        }

        public Contactpersoon Add(Contactpersoon model) {
            var dc = new LcAsterixDataContext(_database);
            var newCard = new AsterixKlantenContactpersonen();
            newCard.CpKlantzoekcode = model.Klantnummer;
            newCard.CpAchternaam = model.Achternaam;
            newCard.CpVoorletters = model.Voorletters;
            newCard.CpVoornaam = model.Voornaam;
            newCard.CpEmail = model.Email;
            newCard.CpFunctie = model.Functie;
            newCard.CpTelefoon1 = model.Telefoon;
            newCard.CpTelefoon2 = model.Telefoon2;
            newCard.CpGeslacht = model.Geslacht;
            newCard.CpIshoofdcontact = model.IsHoofdcontact;
            ObjectFactory.SetDefaultFieldValues(newCard);
            dc.AsterixKlantenContactpersonen.InsertOnSubmit(newCard);
            dc.SubmitChanges();
            model.Id = newCard.Id;
            return model;
        }

        public void Delete(int id) {
            var dc = new LcAsterixDataContext(_database);
            var del = dc.AsterixKlantenContactpersonen.Single(x => x.Id == id);
            dc.AsterixKlantenContactpersonen.DeleteOnSubmit(del);
            dc.SubmitChanges();
        }

        public List<Contactpersoon> FullTextSearch(string filter) {
            filter = filter.Trim().ToLower();
            var filterDate = Helpers.GetFilterDate(filter);
            var dc = new LcAsterixDataContext(_database);
            var ds = dc.AsterixKlantenContactpersonen.Where(x => x.Id.ToString() == filter
                                                                   || x.CpKlantzoekcode.ToLower().IndexOf(filter) != -1
                                                                   || x.CpAchternaam.ToLower().IndexOf(filter) != -1
                                                                   || x.CpVoorletters.ToLower().IndexOf(filter) != -1
                                                                   || x.CpVoornaam.ToLower().IndexOf(filter) != -1
                                                                   || x.CpEmail.ToLower().IndexOf(filter) != -1
                                                                   || x.CpFunctie.ToLower().IndexOf(filter) != -1
                                                                   || x.CpTelefoon1.ToLower().IndexOf(filter) != -1
                                                                   || x.CpTelefoon2.ToLower().IndexOf(filter) != -1
                                                                   || x.CpGeslacht.ToLower().IndexOf(filter) != -1
            );
            var resultSet = new List<Contactpersoon>();
            Parallel.ForEach(ds, card => {
                lock (resultSet) {
                    resultSet.Add(GetModel(card));
                }
            });
            return resultSet;
        }

