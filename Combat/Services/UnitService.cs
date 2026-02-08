using LiteDB;
using Combat.Models;

namespace Combat.Services
{
    public class UnitService
    {
        private const string DbFile = "Units.db";

        public List<Unit> GetAllUnits()
        {
            using var db = new LiteDatabase(DbFile);
            var col = db.GetCollection<Unit>("units");
            return col.FindAll().ToList();
        }

        public void SaveUnit(Unit unit)
        {
            using var db = new LiteDatabase(DbFile);
            var col = db.GetCollection<Unit>("units");

            if (unit.Id == 0)
                col.Insert(unit);
            else
                col.Update(unit);
        }

        public void DeleteUnit(int id)
        {
            using var db = new LiteDatabase(DbFile);
            var col = db.GetCollection<Unit>("units");
            col.Delete(id);
        }
    }
}
