import { Component, OnInit } from '@angular/core';
import { faEnvelope} from '@fortawesome/free-solid-svg-icons';
import {NgbModal, ModalDismissReasons} from '@ng-bootstrap/ng-bootstrap';
import { Korisnik } from '../Korisnik';
import { AppService } from '../services/app.service';
import { FilterKorisnik } from './FilterKorisnik';
import { CookieService } from 'ngx-cookie-service';
import { CV } from '../cv/CV';
import { OglasiComponent } from '../oglasi/oglasi.component';
import { Oglas } from '../oglasi/oglas';
import { AppComponent } from '../app.component';
import { Poruka } from '../inbox/Poruka';
import { FormGroup,FormControl } from '@angular/forms';
import { faUsers } from '@fortawesome/free-solid-svg-icons';
@Component({
  selector: 'app-lista-korisnika',
  templateUrl: './lista-korisnika.component.html',
  styleUrls: ['./lista-korisnika.component.css']
})
export class ListaKorisnikaComponent implements OnInit {

  faEnvelope = faEnvelope; // Ikonica koverte
  faUsers = faUsers; // Ikonica za korisnike

//Pomocne promenjive za UI//
  ImageLoad:boolean; // Provera da li je slika valid
  isEdited:boolean; // Provera da li su filteri ukljuceni
  TipKorisnika:string; // Provera tipa korisnika
//----------------------//

FormaTekst:FormGroup;// Forma za tekst

//Pomocne promenjive za realizaciju funkcionalnosti//
  brKorisnika:number;
  poruka:Poruka;
  pom:Korisnik[];
  oglasi:Oglas[];
  counter:number;
  SelektovaniCV:CV;
  izabrano:Korisnik;
  izabraniOglas:Oglas;
  korisnici:Korisnik[];
  Selektovani:Korisnik;
  korisnik_podaci:Korisnik;
  KorisnikForm:FilterKorisnik;
  //-----------------------------------------------//
 

  constructor(private service:AppService, private modalService: NgbModal,private cookies:CookieService, private app:AppComponent) 
   {  
    this.pom=[];  
    this.oglasi=[];  
    this.counter=0;
    this.korisnici=[]; 
    this.brKorisnika=0;
    this.isEdited=false;
    this.TipKorisnika = "-1";
    this.poruka=new Poruka(); 
    this.izabrano=new Korisnik();
    this.SelektovaniCV = new CV();
    this.izabraniOglas=new Oglas();
    this.Selektovani = new Korisnik(); 
    this.korisnik_podaci=new Korisnik(); 
    this.KorisnikForm=new FilterKorisnik();    
    // Utvrdjivanje tipa korisnika
    this.service.DBGetInfo().subscribe((data)=>
    {
      if(data != null)
      {
        if(data[1] != null)
        {
          this.TipKorisnika = data[1];
        }
      }
    },(error)=>{})  
   }


  ngOnInit(): void 
  {
    // Provera da li je korisnik ulogovan
   if(this.cookies.get("Token")!= "")
   // Provera da li je identitet korisnika dobar
    this.service.DBGetInfo().subscribe((data)=>
    {
      if(data == null)
      {
        this.app.Logout();
      }
    },(error)=>{})
   
    // Inizijalizacija forme
    this.FormaTekst = new FormGroup({
      'Tekst': new FormControl(null)
    });

    //Pribavljanje liste korisnika iz baze podatak
    this.service.DBGetKorisnici().subscribe((data)=>
    {
      this.brKorisnika=0;
      if(data!=null)
      {
        data.forEach(el => {
          this.korisnici.push(el);
          this.brKorisnika++;
          this.service.DBOtherInfoEmail(el.email).subscribe((data)=>
          {
            if(data!=null)
            {
              switch(data[1])
              {
                case "Privatno lice":
                  el.tip = data[1];
                  el.Ime = data[5];
                  el.Prezime = data[6];    
                  break;
        
                  case "Firma":
                    el.tip = data[1];
                    el.Naziv = data[5]; 
                  break;
        
                  case "Radnik":
                    el.tip = data[1];
                    el.Ime = data[4];
                    el.Prezime = data[5]; 
                  break;
        
                  case "Admin":
                    el.tip = data[1];
                    el.Ime = data[4];
                    el.Prezime = data[5];
                  break;
                 default:
                   alert("fail V");
                  break;
              }
            }
          })
        });
      }
    } ,
    (error)=>{});

// Provera validnosti korsinika i pribavljanje liste oglasa
this.service.DBGetInfo().subscribe((dataT)=>
{
 if (dataT != null)
 {
  this.service.DBGetOglas(dataT[0]).subscribe((data)=>
    {
      if(data!=null)
      {
        data.forEach(el => {
          this.oglasi.push(el);
        });
      }
    },(error)=>{});
  }
 },
 (error)=>{}); 
}

// Funkcija za slanje poruka
  posalji(content:Poruka): void
  {
    // Slanje poruka u bazu podataka
    this.service.DBPosaljiPoruku(this.poruka,this.app.User.KorisnickoIme).subscribe((data)=>
    {
      if(data!=null)
      {
        alert("Poruka uspesno poslata!");
        this.close(content);
        this.app.getToPage('Index');
      }
      else
      {
      alert("Greska pri slanju poruke!");
      this.close(content);
      this.app.getToPage('Index');
      }
    },
    (error)=>
    {
      alert("Greksa!");
      this.close(content);
      this.app.getToPage('Index');
    });  
}

// Otvaranje modala za slanje poruka sa profila
  openPorukaProfil(content:any): void
  {
    this.poruka=new Poruka();
    this.poruka.email=this.Selektovani.Email;
    this.modalService.open(content);
   
  }

  // Otvranje modala za slanje poruka sa liste
  openPoruka(event:MouseEvent,content:any,pom:string): void
  {
    if(this.TipKorisnika != "-1")
   {
    this.poruka=new Poruka();
    this.poruka.email=pom;
    event.preventDefault();
    event.stopPropagation();
    this.modalService.open(content);
  }
  }

  // Funkcija za dobavljanje podataka jednog korisinka pri otvaranju modala profila
  open(content:any,obj:Korisnik ): void 
  { 
     this.modalService.open(content);
     this.Selektovani = new Korisnik();
     this.service.DBOtherInfoEmail(obj.email).subscribe((data)=>{
      switch(data[1])
      {
        case "Privatno lice":
          this.Selektovani.KorisnickoIme = data[0];
          this.Selektovani.tip = data[1];
          this.Selektovani.Email = data[2];
          this.Selektovani.Grad = data[3];
          if(data[4].toLowerCase() == "blank")
          this.Selektovani.Kontakt = "";
          else
          this.Selektovani.Kontakt = data[4]
          this.Selektovani.Ime = data[5];
          this.Selektovani.Prezime = data[6];    
          this.Selektovani.imgPath = data[7];  
          break;

          case "Firma":
          this.Selektovani.KorisnickoIme = data[0];
          this.Selektovani.tip = data[1];
          this.Selektovani.Email = data[2];
          this.Selektovani.Grad = data[3];
          if(data[4].toLowerCase() == "blank")
          this.Selektovani.Kontakt = "";
          else
          this.Selektovani.Kontakt = data[4]
          this.Selektovani.Naziv = data[5];
          this.Selektovani.Lokacija = data[6]; 
          this.Selektovani.imgPath = data[7];     
          break;

          case "Radnik":
            this.Selektovani.KorisnickoIme = data[0];
            this.Selektovani.tip = data[1];
            this.Selektovani.Email = data[2];
            this.Selektovani.Grad = data[3];
            this.Selektovani.Ime = data[4];
            this.Selektovani.Prezime = data[5]; 
            this.Selektovani.imgPath = data[6]; 
          break;

          case "Admin":
            this.Selektovani.KorisnickoIme = data[0];
            this.Selektovani.tip = data[1];
            this.Selektovani.Email = data[2];
            this.Selektovani.Grad = data[3];
            this.Selektovani.Ime = data[4];
            this.Selektovani.Prezime = data[5];
            this.Selektovani.imgPath = data[6]; 
          break;
         default:
           alert("fail V");
          break;
      }
    },(error)=>{});
    this.Selektovani.id = obj.id;
   }


   // Funkcija za zatvaranje modala
   close(content:any):void 
  { 
    this.modalService.dismissAll(content);
  }

 // Funkcija za otvaranje modala CV-a
  openCV(content:any):void 
  { 
    this.SelektovaniCV = new CV();
    // Pribavljanje podataka CV-a
    this.service.DBGetOtherCV(this.Selektovani.KorisnickoIme).subscribe((data)=>
    {
      if(data!= null)
      {
        this.SelektovaniCV.godina_iskustva = data.godina_iskustva;
        this.SelektovaniCV.work_experience = data.work_experience;
        this.SelektovaniCV.obrazovanje = data.obrazovanje;
        this.SelektovaniCV.tehnologije = data.tehnologije;
        this.SelektovaniCV.licni_opis = data.licni_opis;
        this.SelektovaniCV.telefon = data.telefon;
        this.SelektovaniCV.jezici = data.jezici;
        this.SelektovaniCV.adresa = data.adresa;
        this.SelektovaniCV.email = data.email; 
        this.modalService.open(content);
      }    
    },
    (error)=>
    {
      this.close(content);
      alert("Korisnik ne poseduje adekvatan CV !");
    }
    ) 
  
  }

  // Funckija za otvaranje modala
  openOglas(content:any): void
  {
    this.modalService.open(content);
  }

  //Funkcija za slanje zahteva radniku
  posaljiZahtev(): void
  {
    this.service.DBPosaljiZahtev(this.Selektovani.id,this.izabraniOglas.id,this.app.User.KorisnickoIme).subscribe((data)=>
    {
      if(data)
      {
        alert("Zahtev uspesno poslat!");
        this.app.getToPage('Index');
      }
      else
      {
      alert("Greska pri slanju zahteva!");
      this.app.getToPage('Index');
      }
    },
    (error)=>
    {
      alert("Vec ste poslali zahtev za dati oglas!");
      this.app.getToPage('Index');
    });
  }

  // Funkcijza za brisanje korisnika od strane administratora
  ObrisiKorisnika()
  {
    // Provera identiteta administratora i brisanje 
    this.service.DBGetInfo().subscribe((dataT)=>
    {  if(dataT != null)
      {
       if(dataT[1] == "Admin")
        {
           var dialog = confirm("Potvrdi brisanje korisnika?");
          if (dialog == true) 
          {
           this.service.DBDeleteKorisnika(this.Selektovani.KorisnickoIme).subscribe((data)=>
          {  
            if(data)
            {
              location.reload();
              alert("Korisnik obrisan")
            }
          },(error)=>{});
          }
        }
      }
      }, (error)=>{}
      );
  }


// Kreiranje putanje slike iz baze podataka
  public createImgPath = (serverPath: string) => 
  { 
  return `https://localhost:5001/${serverPath}`; 
  }

// Funkcija za pretragu 
  PretraziKorisnika() :void
  {
    this.isEdited=true;
    while(this.counter>0)
      {
      this.pom.pop();
      this.counter--;
      }
       this.service.DBGetFilterKorisnici(this.KorisnikForm).subscribe((data)=>{
       data.forEach(el => {
       this.counter++;
       this.service.DBOtherInfoEmail(el.email).subscribe((data)=>
          {
            if(data!=null)
            {
              switch(data[1])
              {
                case "Privatno lice":
                  el.tip = data[1];
                  el.Ime = data[5];
                  el.Prezime = data[6];    
                  break;
        
                  case "Firma":
                    el.tip = data[1];
                    el.Naziv = data[5]; 
                  break;
        
                  case "Radnik":
                    el.tip = data[1];
                    el.Ime = data[4];
                    el.Prezime = data[5]; 
                  break;
        
                  case "Admin":
                    el.tip = data[1];
                    el.Ime = data[4];
                    el.Prezime = data[5];
                  break;
                 default:
                   alert("fail V");
                  break;
              }
            }
          })
       this.pom.push(el);
      });
    },(error)=>{});
  }
}

