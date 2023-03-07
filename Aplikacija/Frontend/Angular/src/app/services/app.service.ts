import { EventEmitter, Injectable, Output } from '@angular/core';
import { Component, Input } from '@angular/core';
import { RegisterUser } from 'src/app/RegisterUser'
import { HttpClient } from '@angular/common/http';
import { CookieService } from 'ngx-cookie-service';
import { HttpHeaders } from '@angular/common/http';
import { Korisnik } from '../Korisnik';
import { CV } from 'src/app/cv/CV'
import { Oglas } from 'src/app/oglasi/oglas';
import { FilterOglas } from '../lista-oglasa/FilterOglas';
import { FilterKorisnik } from '../lista-korisnika/FilterKorisnik';
import { Poruka } from '../inbox/Poruka';


@Injectable({
  providedIn: 'root'
})
  export class AppService {
    readonly URL = "https://localhost:5001/";
    r:string;
    filterPom1:string;
    filterPom2:string;
    filterPom3:string;
    filterpom4:number;
    filterpom5:number;
    filterpom6:string;
    filterKorisnik1:string;
    filterKorisnik2:string;
  constructor(private http:HttpClient,private cookies:CookieService )
{
  this.r="";
  this.filterPom1="";
  this.filterPom2="";
  this.filterPom3="";
  this.filterpom6="";
  this.filterKorisnik1="";
  this.filterKorisnik2="";
}

DBRegister(obj:RegisterUser)
{
    var TipDB:number = -1;
    var PoslodavacTipDB:number = -1;
    obj.Kontakt =obj.Email;
    if(obj.Tip == "Radnik")
    {
         TipDB = 0;
         obj.Lokacija ="blank";
         obj.Naziv ="blank";
         PoslodavacTipDB = 1;
    }
    else
    { 
     
        TipDB = 1;
         if(obj.Tip == "Privatno lice" )
         {
          obj.Naziv ="blank";
          obj.Lokacija ="blank";
          PoslodavacTipDB = 0;     
         }
        
         else if (obj.Tip == "Firma" )
         {
            PoslodavacTipDB = 1;
            obj.Ime ="blank";
            obj.Prezime ="blank";
         }
     }
  obj.KorisnickoIme = this.DeleteAll(obj.KorisnickoIme);
  obj.Ime = this.DeleteAll(obj.Ime);
  obj.Prezime = this.DeleteAll(obj.Prezime);
  obj.Lokacija = this.DeleteAll(obj.Lokacija);
  obj.Naziv = this.DeleteAll(obj.Naziv);
  obj.Kontakt = this.DeleteAll(obj.Kontakt);
    return  this.http.post(this.URL+`Korisnik/AddKorisnik/${this.ReplaceAll(obj.KorisnickoIme)}/${this.ReplaceAll(obj.Lozinka)}/${obj.Email}/${TipDB}/${obj.Grad}/${this.ReplaceAll(obj.Ime)}/${this.ReplaceAll(obj.Prezime,)}/${PoslodavacTipDB}/${this.ReplaceAll(obj.Lokacija)}/${this.ReplaceAll(obj.Naziv)}/${this.ReplaceAll(obj.Kontakt)}`,obj);
}

DBCheckData(korisnicko_ime:string,email:string)
{
  return this.http.get(this.URL+`Korisnik/ProveriPostojanjePodataka/${korisnicko_ime}/${email}`);
}


DBLogin(korisnicko_ime:string,lozinka:string)
{
   return this.http.get(this.URL+`Korisnik/Login/${korisnicko_ime}/${lozinka}`,{ responseType: 'text' });
}

DBCheckType(korisnicko_ime:string,lozinka:string)
{
  return this.http.get(this.URL+`Korisnik/ProveriTip/${korisnicko_ime}/${lozinka}`,{ responseType: 'text' });
}

DBGetInfo()
{
  var headers = new HttpHeaders().set("Authorization", "Bearer " + this.cookies.get("Token"));
 return this.http.get<string[]>(this.URL+`Korisnik/VratiInfo`,{ headers: headers}); 
}

DBChangeInfo(obj:Korisnik)
{

  var PoslodavacTipDB:number = -1;
  var TipDB:number = -1;
switch(obj.tip)
{
  case "Privatno lice" :
    obj.Lokacija ="blank";
    obj.Naziv ="blank";
    PoslodavacTipDB = 0; 
    TipDB = 1;
    break;

  case "Radnik" :
    obj.Lokacija ="blank";
    obj.Kontakt ="blank";
    obj.Naziv ="blank";
    TipDB = 0;
    PoslodavacTipDB = 0;
    break;

  case "Firma" :
    PoslodavacTipDB = 1;
    TipDB = 1;
    obj.Ime ="blank";
    obj.Prezime ="blank";
    break;

    case "Admin" :
    obj.Lokacija ="blank";
    obj.Kontakt ="blank";
    obj.Naziv ="blank";
    TipDB = 3;
    PoslodavacTipDB = 0;
    break;

  default:
    break;

}

  if(obj.tip == "Privatno lice" || obj.tip == "Radnik")
  {
    obj.Naziv ="blank";
    obj.Lokacija ="blank";
    PoslodavacTipDB = 0;   
  }
  obj.KorisnickoIme = this.DeleteAll(obj.KorisnickoIme);
  obj.Ime = this.DeleteAll(obj.Ime);
  obj.Prezime = this.DeleteAll(obj.Prezime);
  obj.Lokacija = this.DeleteAll(obj.Lokacija);
  obj.Naziv = this.DeleteAll(obj.Naziv);
  obj.Kontakt = this.DeleteAll(obj.Kontakt);
  var headers = new HttpHeaders().set("Authorization", "Bearer " + this.cookies.get("Token"));
  return this.http.put(this.URL+`Korisnik/ChangeUserInfo/${obj.KorisnickoIme}/${TipDB}/${obj.Grad}/${obj.Ime}/${obj.Prezime}/${PoslodavacTipDB}/${obj.Lokacija}/${obj.Naziv}/${obj.Kontakt}`,{headers: headers}); 
}

DBChangePass(email:string,lozinka:string, nova:string)
{
  var headers = new HttpHeaders().set("Authorization", "Bearer " + this.cookies.get("Token"));
  return this.http.put(this.URL+`Korisnik/ChangePassword/${email}/${this.ReplaceAll(lozinka)}/${nova}`,{headers: headers}); 
}


DBAddCV(obj:CV,KorisnickoIme:string)
{
  obj.adresa = this.DeleteAll(obj.adresa);
  obj.telefon = this.DeleteAll(obj.telefon);
  obj.telefon = this.MakePositive(obj.telefon);
  var headers = new HttpHeaders().set("Authorization", "Bearer " + this.cookies.get("Token"));
  return this.http.post(this.URL+`CV/AddCV/${KorisnickoIme}/${this.ReplaceAll(obj.obrazovanje)}/${obj.godina_iskustva}/${this.ReplaceAll(obj.jezici)}/${this.ReplaceAll(obj.tehnologije)}/${this.ReplaceAll(obj.work_experience)}/${this.ReplaceAll(obj.licni_opis)}/${this.ReplaceAll(obj.email)}/${this.ReplaceAll(obj.adresa)}/${this.DeleteAll(obj.telefon)}`,{headers: headers}); 
}

DBGetCV(KorisnickoIme:string) 
{
  var headers = new HttpHeaders().set("Authorization", "Bearer " + this.cookies.get("Token"));
  return this.http.get<CV>(this.URL+`CV/GetCV2/${KorisnickoIme}`,{headers: headers}); 
}

DBGetOglas(KorisnickoIme:string)
{
  var headers = new HttpHeaders().set("Authorization", "Bearer " + this.cookies.get("Token"));
  return this.http.get<Oglas[]>(this.URL+`Oglasi/GetOglas/${KorisnickoIme}`,{headers: headers}); 
}

DBGetOglasi()
{
  return this.http.get<Oglas[]>(this.URL+`Oglasi/PrikaziOglase`); 
}

DBGetFilterOglasi(obj:FilterOglas)
{
  if(obj.kljucna_rec=="") this.filterPom1="null"; else this.filterPom1=obj.kljucna_rec; 
  if(obj.lokacija=="") this.filterPom2="null"; else this.filterPom2=obj.lokacija;
  if(obj.tehnologija=="") this.filterPom3="null"; else this.filterPom3=obj.tehnologija;
  if(obj.max_plata==null || obj.max_plata == undefined || obj.max_plata == NaN) this.filterpom5=0; else this.filterpom5=obj.max_plata;
  if(obj.min_plata==null || obj.max_plata == undefined || obj.max_plata == NaN) this.filterpom4=0; else this.filterpom4=obj.min_plata;
  if(obj.remote_work=="") this.filterpom6="null"; else this.filterpom6=obj.remote_work; 

  if(obj.max_plata <= 0) {this.filterpom5=0;obj.max_plata = 0; }
  if(obj.min_plata < 0) {this.filterpom4=0;obj.min_plata = 0;} 
     
  return this.http.get<Oglas[]>(this.URL+`Oglasi/GetOglasi/${this.ReplaceAll(this.filterPom3)}/${this.filterpom4}/${this.filterpom5}/${this.filterpom6}/${this.DeleteAll(this.filterPom2)}/${this.ReplaceAll(this.filterPom1)}`);   
}


DBGetKorisnici()
{
  return this.http.get<Korisnik[]>(this.URL+`Korisnik/PrikaziKorisnike`); 
}

DBGetFilterKorisnici(obj:FilterKorisnik)
{    
  if(obj.kljucna_rec=="") this.filterKorisnik1="null"; else this.filterKorisnik1=obj.kljucna_rec; 
  if(obj.tip=="") this.filterKorisnik2="null"; else this.filterKorisnik2=obj.tip;
  return this.http.get<Korisnik[]>(this.URL+`Korisnik/PrikaziKorinika/${this.filterKorisnik1}/${this.filterKorisnik2}`); 
}


DBUpdateOglas(obj:Oglas)
{
  obj.lokacija = this.DeleteAll(obj.lokacija);
  return this.http.put(this.URL+`Oglasi/ChangeOglas/${obj.id}/${this.ReplaceAll(obj.naziv)}/${this.ReplaceAll(obj.opis)}/${this.ReplaceAll(obj.tehnologija)}/${this.DeleteAll(obj.lokacija)}/${obj.remote_work}/${obj.plata}`,obj); 
}

DBDodajOglas(obj:Oglas,KorisnickoIme:string)
{
  if(obj.plata <0)
  obj.plata=obj.plata*(-1);
  obj.lokacija = this.DeleteAll(obj.lokacija);
  var headers = new HttpHeaders().set("Authorization", "Bearer " + this.cookies.get("Token"));
  return this.http.post(this.URL + `Oglasi/AddOglas/${this.ReplaceAll(obj.naziv)}/${this.ReplaceAll(obj.tehnologija)}/${this.ReplaceAll(obj.opis)}/${obj.plata}/${obj.remote_work}/${this.DeleteAll(obj.lokacija)}/${KorisnickoIme}`,{headers:headers});
}

DBOtherInfo(ID:number)
{
  return this.http.get<string>(this.URL+`Korisnik/VratiInfoID/${ID}`); 
}

DBOtherInfoEmail(Email:string)
{
  return this.http.get<string>(this.URL+`Korisnik/VratiInfoEmail/${Email}`); 
}


DBGetOtherCV(korisnicko_ime:string)
{
 return this.http.get<CV>(this.URL+`CV/GetCV2/${korisnicko_ime}`); 
}

DBObrisiOglas(obj:Oglas){
  return this.http.delete(this.URL+`Oglasi/DeleteOglas/${obj.id}`)
}


DBObrisiOglasAdmin(obj:Oglas,kIme:string)
{
  return this.http.delete(this.URL+`Admin/ObrisiOglasAdmin/${obj.id}/${kIme}`);
}
    
DBPosaljiZahtev(primaoc:number,oglas:number,KorisnickoIme:string)
{
  return this.http.post(this.URL+`Zahtevi/Send2/${KorisnickoIme}/${primaoc}/${oglas}`,null);
}


DBDeleteKorisnika(korisnicko_ime:string)
{
  return this.http.delete(this.URL+`Admin/DeleteKorisnika/${korisnicko_ime}`);
}

DBPosaljiZahtevZaOglas(id_oglasa:number,KorisnickoIme:string)
{
  return this.http.post(this.URL+`Zahtevi/SendZahtev/${KorisnickoIme}/${id_oglasa}`,null);
}

DBVratiZahteve(korisnicko_ime:string)
{
  return this.http.get<string[]>(this.URL+`Zahtevi/VratiListuZahteva/${korisnicko_ime}`);
}

DBVratiPotvrdjene(KorisnickoIme:string)
{
  return this.http.get<string>(this.URL+`Zahtevi/VratiPotvrdjene/${KorisnickoIme}`);
}

DBVratiIme(id:number)
{
  return this.http.get<string[]>(this.URL+`Korisnik/GetName/${id}`)
}

DBOdgovori(id:number,odgovor:number)
{
  return this.http.put(this.URL+`Zahtevi/ChangeStatus/${id}/${odgovor}`,null)
}

DBGetOglasName(id:number)
{
  return this.http.get(this.URL+`Oglasi/GetOglasNaslov/${id}`,{ responseType: 'text' });
}

DBGetOglasInfoID(id:number)
{
  return this.http.get<Oglas[]>(this.URL+`Oglasi/GetOglasID/${id}`);
}
DBPosaljiPoruku(obj:Poruka,KorisnickoIme:string)
{
  var headers = new HttpHeaders().set("Authorization", "Bearer " + this.cookies.get("Token"));
  return this.http.post(this.URL+`PrivateMessage/Send/${this.ReplaceAll(obj.naslov)}/${this.ReplaceAll(obj.poruka)}/${KorisnickoIme}/${obj.email}`,{headers:headers});
}
DBVratiPoslatePoruke(KorisnickoIme:string) 
{
  return this.http.get<Poruka[]>(this.URL+`PrivateMessage/GetPoslate/${KorisnickoIme}`);
}
DBVratiPrimljenePoruke(KorisnickoIme:string)
{
  
  return this.http.get<Poruka[]>(this.URL+`PrivateMessage/GetPrimljene/${KorisnickoIme}`);
}
DBObrisiPoruku(id:number)
{
  return this.http.delete(this.URL+`PrivateMessage/DeleteMessage/${id}`);
}

ReplaceAll(Tekst:string)
{
for(let i=0 ; i <= Tekst.length; i++)
{
  if(Tekst[i]=="#")
  {
    Tekst = Tekst.replace("#","01abfc750a0c942167651c40d088531d");
  }
}
return Tekst;
}

DeleteAll(Tekst:string)
{
for(let i=0 ; i <= Tekst.length; i++)
{
  if(Tekst[i]=="#")
  {
    Tekst = Tekst.replace("#","");
  }
}
return Tekst;
}

MakePositive(Tekst:string)
{
 let Number = parseInt(Tekst);
 if(Number < 0)
 Number = Number * (-1);
 Tekst = Number + "";
 return Tekst;
}

}
