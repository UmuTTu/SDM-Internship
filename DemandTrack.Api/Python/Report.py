import csv
import sqlite3

Db_Path = "DemandTrack.Api/Demand.db"
Csv_Path = "demands.csv"

conn = sqlite3.connect(Db_Path)
cursor = conn.execute("SELECT * FROM Demands")
titles = [colums[0] for colums in cursor.description]
rows = cursor.fetchall()
conn.close()

with open(Csv_Path, "w", newline="", encoding="utf-8-sig") as f:
    writer = csv.writer(f)
    writer.writerow(titles)
    writer.writerows(rows)

#CSV'den oku ve say
with open(Csv_Path, encoding="utf-8-sig") as f:
    talepler = list(csv.DictReader(f))

def count(durum):
    counter = 0
    for i in range(len(talepler)):
        if talepler[i]["Durum"].lower() == durum.lower():
            counter += 1
    return counter

print("Toplam talep sayısı:", len(talepler))
print("Onaylanan talep sayısı:", count("Onaylandı"))
print("Reddedilen talep sayısı:", count("Reddedildi"))
print("Revizyon bekleyen talep sayısı:", count("Revizyon Bekliyor"))
print("Yeni talep sayısı:", count("Yeni"))
