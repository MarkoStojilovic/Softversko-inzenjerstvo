import { Component, OnInit } from '@angular/core';
import { AppService } from '../services/app.service';
import { Oglas } from 'src/app/oglasi/oglas';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { FilterOglas } from './FilterOglas';
import { CookieService } from 'ngx-cookie-service';
import { AppComponent } from '../app.component';
import { faFileLines } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-lista-oglasa',
  templateUrl: './lista-oglasa.component.html',
  styleUrls: ['./lista-oglasa.component.css']
})
export class ListaOglasaComponent implements OnInit {

  faFileLines = faFileLines; // Ikonica za oglase

//Pomocne promenjive za UI i funkcionalnosti//
  i:boolean;
  pom:Oglas[];
  index:number;
  TipView:string;
  oglasi:Oglas[];
  Filter:Oglas[];
  counter:number;
  izabrano:Oglas;
  brOglasa:number;
  StariNiz:Oglas[];
  isEdited:boolean;
  Oglas_podaci:Oglas;
  OglasForm:FilterOglas; 
  SelektovaniOglas:Oglas;
  filtriraniOglasi:number;
//----------------------------------------//

  constructor(private service:AppService, private modalService: NgbModal,private cookies:CookieService,private app:AppComponent) 
   { 
    this.pom=[];
    this.oglasi=[];
    this.Filter=[];
    this.StariNiz=[];
    this.TipView = "";
    this.brOglasa = 0;
    this.izabrano=new Oglas();
    this.filtriraniOglasi = 0;
    this.Oglas_podaci=new Oglas(); 
    this.OglasForm=new FilterOglas();
    this.SelektovaniOglas = new Oglas();
  
    // Provera tipa korisnika
    this.service.DBGetInfo().subscribe((data)=>
    {
      if(data != null)
      {
       
        if(data[1] == "Radnik")
        {     
          this.TipView = "Radnik";  
        }
        else if(data[1] == "Admin")
        {
          this.TipView = "Admin";   
        }

      }

    } ,
    (error)=>{})
  }

  ngOnInit(): void 
  {
    // Provera da li je korisnik ulogovan i da li je njegov token validan
    if(this.cookies.get("Token")!= ""){
      this.service.DBGetInfo().subscribe((data)=>
      {
        if(data == null)
        {
          this.app.Logout();
        }
      } ,
      (error)=>{})
     }
    this.isEdited=false;
    this.counter=0;
     // Pribavljanje oglasa iz baze podataka
        this.service.DBGetOglasi().subscribe((data)=>
       {
          this.brOglasa=0;
           if(data!=null)
       {
          data.forEach(el => {
          this.oglasi.push(el);
          this.StariNiz.push(el); 
          this.brOglasa++;
          this.filtriraniOglasi++
        });
      }
    } ,
    );  
  }


// Pretrezivanje oglasa 
Pretrazi(): void
{
  this.filtriraniOglasi = 0;
  this.isEdited=true;
  while(this.counter>0)
    {
      this.pom.pop();
      this.counter--;
    }
  this.service.DBGetFilterOglasi(this.OglasForm).subscribe((data)=>{
    data.forEach(el => {
     this.counter++;
     this.filtriraniOglasi++;
     this.pom.push(el);
    });
  },(error)=>{});
}

// Funkcija za zatvaranje modala
close(content:any): void
{ 
  this.modalService.dismissAll(content);
}

// Funkcija za otvaranje modala za oglase
openOglas(content:any,data:Oglas): void
{ 
  this.modalService.open(content);
  this.SelektovaniOglas = data;
}



ObrisiOglas(): void
{
  var korisnicko_ime:string;
  korisnicko_ime="";
  // Provera identiteta korisnika i brisanje selektovanog oglasa
  this.service.DBGetInfo().subscribe((data)=>
  {
    if(data!=null)
    if(data[1] == "Admin")
    {
      korisnicko_ime=data[0];
      var dialog = confirm("Potvrdi brisanje oglasa?");
      if (dialog == true) 
      this.service.DBObrisiOglasAdmin(this.SelektovaniOglas,korisnicko_ime).subscribe((data1)=>
      {
       if(data1 == true)
       location.reload();
      },(error)=>{});
    }  
  },(error)=>{});
}


// Funkcija za apliciranje na oglas
AplicirajOglas():void
 {
 this.service.DBPosaljiZahtevZaOglas(this.SelektovaniOglas.id,this.app.User.KorisnickoIme)?.subscribe(
    (data)=>
  {
      alert("Uspesno poslat zahtev !")
  },
  (error)=>
  {
    alert("Vec ste poslali zahtev za dati oglas!");
  });
 }
}
