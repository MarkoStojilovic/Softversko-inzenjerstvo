import { Component, OnInit, Input ,ElementRef} from '@angular/core';
import { AppService } from '../services/app.service';
import { FormsModule, FormGroup, NgForm ,FormControl} from '@angular/forms';
import { Oglas } from './oglas';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ViewChild } from '@angular/core'
import { CookieService } from 'ngx-cookie-service';
import {AppComponent} from '../app.component'

@Component({
  selector: 'app-oglasi',
  templateUrl: './oglasi.component.html',
  styleUrls: ['./oglasi.component.css']
})
export class OglasiComponent implements OnInit {

//Forme za prikupljanje teksta//
  FormaTehnologije:FormGroup;
  FormaOpis:FormGroup;
//---------------------------//

//Pomocne promenjive za UI i funkcionalnosti//  
  rmwork:string;
  izabrano:Oglas;
  oglasi:Oglas[];
  EditMode:boolean;
  UserType:boolean;
  brojOglasa:number;
  ErrorState:number;  
  dodajOglas:boolean;
//-----------------------------------------//



  @ViewChild("test",{static:true}) content:ElementRef;
  
   constructor(private service:AppService, private modalService: NgbModal, private cookies:CookieService, private app:AppComponent) 
   { 
    this.izabrano=new Oglas();
    this.EditMode = false;
    this.oglasi=[];
    this.rmwork="";
    if(this.cookies.get("dodajOglas")=="true")
    this.dodajOglas=true;
    else
    this.dodajOglas=false;
    this.cookies.delete("dodajOglas");
    this.UserType = false;  
   }

  ngOnInit(): void 
  {
    // Provera da li je korisnik ulogovan
    if(this.cookies.get("Token")=="")
    {
      this.app.IndexPage = true;
      this.app.getToPage("Index");
    }

    this.brojOglasa=0;
    if(this.dodajOglas==false)
    this.open(this.content)
    else
    {
      // Funkcija za proveru tipa korisnika i autorizacija
      this.service.DBGetInfo().subscribe((data)=>
      {
        if(data != null)
        {        
         if(data[1] != "Firma" && data[1] != "Privatno lice")
          {
            this.app.IndexPage = true;
            this.app.getToPage("Index");
            this.UserType = false;
          }
          else
          {
           this.UserType = true;
          }        
        }
        else
        {
         this.UserType = false;
         this.app.Logout();
        }
      },(error)=>{});
    }
    
    // Inicijalizacija formi
   this.FormaOpis = new FormGroup({
      'Opis': new FormControl(null)
    });
    this.FormaTehnologije = new FormGroup({
      'Tehnologije': new FormControl(null)
    });

    //Pribavljanje oglasa trenutno prijavljenog korisnika
    this.service.DBGetInfo().subscribe((data)=>
    {
      if(data != null)
      {
          this.service.DBGetOglas(data[0]).subscribe((data2)=>
          {
            if(data2!=null)
            {
              data2.forEach(el => {
                this.oglasi.push(el);
                this.brojOglasa=this.brojOglasa+1;
              });
              alert("Vas broj oglasa je "+this.brojOglasa+". Maksimalno je moguce imati 5 oglasa!");
            }    
          });
      }
      else
      {
       this.app.Logout();
      }
    } ,
    (error)=>{}); 
  }


// Funkcija za memorisanje oglasa
  sacuvajOglas(): void
  {
    if(this.izabrano.plata == 0)
    {
      alert("Vrednost plate mora biti veca od 0 !");
      return;
    }
    this.app.oglasDugme=true;
    this.cookies.delete("dodajOglas");
    this.ErrorState=-1;
    if(this.brojOglasa<5)
    {
    if(this.izabrano.naziv == "" || this.izabrano.tehnologija == "" || this.izabrano.opis == "" || this.izabrano.plata  == 0 || this.izabrano.lokacija == "" || this.rmwork == "")
    {
    this.ErrorState = 1;
    alert("Popunite sve podatke");
    return;
    }
    else {
    this.izabrano.remote_work=this.rmwork;
    this.service.DBDodajOglas(this.izabrano,this.app.User.KorisnickoIme).subscribe((data)=>{},(error)=>{});
  }

  if(this.ErrorState <0)
  alert("Oglas uspesno dodat");
  }
  else
  alert("Nije moguce dodati oglase, dostigli ste maksimum!");
  this.app.getToPage("Index");
  }


  // Funkcija za otvaranje modala
  open(content:any) 
  { 
    // Provera tipa validnosti korisnika
   this.service.DBGetInfo().subscribe((data)=>
   {
     if(data != null)
     {
      if(data[1] != "Firma" && data[1] != "Privatno lice")
       {
         this.app.IndexPage = true;
         this.app.getToPage("Index");
         this.UserType = false;
       }
       else
       {
        this.modalService.open(content);
        this.UserType = true;
       } 
     }
     else
     {
      this.UserType = false;
     }
   } ,(error)=>{});   
  }


// Funkcija za zatvaranje modala
  close(content:any) 
 { 
  this.modalService.dismissAll(content);
 }

 // Funkcij za testiranje objekta
  onClick(){ 
    console.log("Objekat je"+this.izabrano.naziv);
  }

  // Funkcija za otkazivanje dodavanja novog oglasa
  otkaziDodavanje()
  {
    this.app.oglasDugme=true;
    this.app.getToPage('Index');
  }

  // Funkcija za brisanje oglasa
  Obrisi()
  {
    this.app.oglasDugme=true;
    if(confirm("Da li ste sigurni?"))
    {
    this.service.DBObrisiOglas(this.izabrano).subscribe((data)=>{},(error)=>{});
  this.app.getToPage('Index');
  }
  else
  this.app.getToPage('Index');

  }

  // Funkcija za pokretanje UI za izmenu podataka
  EditText()
  {
    this.EditMode = true;
    this.ErrorState = -1;
  }
  
  // Funkcija za memorisanje izmena
  SaveText()
  {
    this.ErrorState=-1;
    if(this.izabrano.id==0 ||  this.izabrano.naziv == "" || this.izabrano.tehnologija == "" || this.izabrano.opis == "" || this.izabrano.plata  == 0  || this.izabrano.lokacija == "" || this.rmwork == "")
   {
    this.ErrorState = 1;
    alert("Popunite sve podatke");
    return; 
   }
    else {
    this.izabrano.remote_work=this.rmwork;
    this.service.DBUpdateOglas(this.izabrano).subscribe((data)=>{},(error)=>{});
  }
  if(this.ErrorState <0)
  this.EditMode = false;
  }
}
