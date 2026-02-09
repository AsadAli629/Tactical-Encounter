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


    public class PartyMemberService
    {
        private const string DbFile = "Units.db";

        public List<PartyMember> GetAll()
        {
            using var db = new LiteDatabase(DbFile);
            return db.GetCollection<PartyMember>("party_members").FindAll().ToList();
        }

        public void Add(PartyMember member)
        {
            using var db = new LiteDatabase(DbFile);
            db.GetCollection<PartyMember>("party_members").Insert(member);
        }

        public void Clear()
        {
            using var db = new LiteDatabase(DbFile);
            db.GetCollection<PartyMember>("party_members").DeleteAll();
        }

        public void Update(PartyMember member)
        {
            using var db = new LiteDatabase(DbFile);
            var col = db.GetCollection<PartyMember>("party_members");
            col.Update(member);
        }

        public void Delete(int id)
        {
            using var db = new LiteDatabase(DbFile);
            var col = db.GetCollection<PartyMember>("party_members");
            col.Delete(id);
        }
    }

    public class CombatLogService
    {
        private const string DbFile = "Units.db";

        public void Add(string message)
        {
            using var db = new LiteDatabase(DbFile);
            db.GetCollection<CombatLog>("combat_logs")
              .Insert(new CombatLog { Time = DateTime.Now, Message = message });
        }

        public void Clear()
        {
            using var db = new LiteDatabase(DbFile);
            db.GetCollection<CombatLog>("combat_logs").DeleteAll();
        }

        public List<CombatLog> GetAll()
        {
            using var db = new LiteDatabase(DbFile);
            return db.GetCollection<CombatLog>("combat_logs")
                     .FindAll()
                     .OrderBy(l => l.Time)
                     .ToList();
        }




    }



}



    

