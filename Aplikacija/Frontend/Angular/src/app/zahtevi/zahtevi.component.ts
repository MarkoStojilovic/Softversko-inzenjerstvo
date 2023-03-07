import { Component, Input,ErrorHandler, OnInit,ViewChild,ElementRef  } from '@angular/core';
import { Korisnik } from '../Korisnik';
import { Oglas } from '../oglasi/oglas';
import { Zahtev } from './Zahtev';
import { AppService } from '../services/app.service';
import { CookieService } from 'ngx-cookie-service';
import { data } from 'jquery';
import { faEnvelope} from '@fortawesome/free-solid-svg-icons';
import { CV } from '../cv/CV';
import {NgbModal, ModalDismissReasons} from '@ng-bootstrap/ng-bootstrap';
import { Poruka } from '../inbox/Poruka';
import { FormsModule,NgForm,FormGroup,FormControl } from '@angular/forms';
import { AppComponent } from '../app.component';

@Component({
  selector: 'app-zahtevi',
  templateUrl: './zahtevi.component.html',
  styleUrls: ['./zahtevi.component.css']
})
export class ZahteviComponent implements OnInit {

  faEnvelope = faEnvelope; // Ikonica za kovertu
  FormaTekst:FormGroup; // Forma za pisanje poruka

  //Pomocne promenjive za UI i funkcionalnosti// 
  ZahteviTip:boolean; // false - "Pristigli zahtevi" | true - Zahtevi koji su prihvaceni
  SelektovaniOglas:Oglas;
  ListaZahteva:Zahtev[];
  Korisnik:Korisnik;
  UserType:boolean;
  SelektovaniCV:CV;
  IsEmpty:boolean;
  ViewTip:string;
  poruka:Poruka;
 //----------------------------------------//
  
 

  constructor(private DataBase:AppService,public cookies:CookieService,private modalService: NgbModal, private app:AppComponent) 
  {  
    this.UserType = false;
    this.ListaZahteva = [];
    this.ZahteviTip = false;
    this.poruka=new Poruka(); 
    this.SelektovaniCV = new CV();  
    this.Korisnik = new Korisnik();
    this.SelektovaniOglas = new Oglas();
    this.ViewTip = this.cookies.get("Tip");  
  }


  ngOnInit(): void 
  {
    // Provera da li je korisnik ulogovan
    if(this.cookies.get("Token")=="")
    {
      this.app.IndexPage = true;
      this.app.getToPage("Index");
    }

    // Provera identiteta korisnika
    this.DataBase.DBGetInfo().subscribe((data)=>
    {
      if(data != null)
      {
       
       if(data[1] != "Firma" && data[1] != "Privatno lice" && data[1] != "Radnik")
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
    } ,
    (error)=>{});

    // Inicijalizacija forme
    this.FormaTekst = new FormGroup({
      'Tekst': new FormControl(null)
    });


    // Provera koji je tip zahteva treba pribaviti
    switch(this.ZahteviTip)
    {
      case false:
        this.GetReceivedZahteve();
      break;
      case true:
        this.GetPositiveZahteve();
      break;
      default:
      break;
    }
   
  }


  // Prebacivanje pogleda sa jedne vrste zahteva na drugu
  PrebaciView(): void
  {
    this.ZahteviTip = true;
    this.GetPositiveZahteve(); 
    this.SelektovaniCV = new CV();
    this.Korisnik = new Korisnik();
    this.SelektovaniOglas = new Oglas();
  }


//Prebacivanje sa druge vrste zahteva na prvu
  PrebaciViewTwo(): void
  {
    this.ZahteviTip = false;
    this.GetReceivedZahteve();
    this.SelektovaniCV = new CV();
    this.Korisnik = new Korisnik();
    this.SelektovaniOglas = new Oglas(); 
  }

  // Funkcija za pribavljanje zahteva koji su poslati trenutnom korisniku
  GetReceivedZahteve(): void
  {
    this.ListaZahteva = [];
    // Provera identiteta korisnika
    this.DataBase.DBGetInfo().subscribe((dataT)=>
  {
   if(dataT != null)
   {
     // Pribavljanje date vrste zahteva
    this.DataBase.DBVratiZahteve(dataT[0]).subscribe((data)=>
    {
      if(data!= null)
      {
        for(var i = 0, j = 0 ; i < data.length ; i++,j++)
        {
          let t = ""
          this.ListaZahteva[j] = new Zahtev();  
          this.ListaZahteva[j].id = data[i++];
          this.ListaZahteva[j].status = data[i++];       
          this.ListaZahteva[j].id_posiljaoca = data[i++];
          this.ListaZahteva[j].id_primaoca = data[i++];
          this.ListaZahteva[j].id_oglasa = data[i];  
          this.GetNames(j,true);
          this.GetOglasName(j);                      
        }
      }
    },
(error)=>{});
  }
},(error)=>{});
  
}


// Funkcija za pribavljanje imena primaoca ili posiljaoca u zavisnosti od tipa zahteva
GetNames(i:number,Tip:boolean): void
{
  if(Tip)
  this.DataBase.DBVratiIme(parseInt(this.ListaZahteva[i].id_posiljaoca)).subscribe((dataIme)=>
  {
   
    if(dataIme.length == 2)
    this.ListaZahteva[i].ime_posiljaoca = dataIme[0] +" "+dataIme[1];
    else
    this.ListaZahteva[i].ime_posiljaoca = dataIme[0];
  },
  (error)=>{});
  else 
  this.DataBase.DBVratiIme(parseInt(this.ListaZahteva[i].id_primaoca)).subscribe((dataIme)=>
  {
    if(dataIme.length == 2)
    this.ListaZahteva[i].ime_primaoca = dataIme[0] +" "+dataIme[1];
    else
    this.ListaZahteva[i].ime_primaoca = dataIme[0];
  },
  (error)=>{});
}


// Funkcija za pribavljanje ime oglasa za koji je poslat zahtev
GetOglasName(i:number): void
{
  this.DataBase.DBGetOglasName(parseInt(this.ListaZahteva[i].id_oglasa)).subscribe((data)=>
  {
    this.ListaZahteva[i].naziv_oglasa = data;
  } ,(error)=>{});
}


// Funkcija za pribavljanje prihvacenih zahteva
  GetPositiveZahteve(): void
  {
    this.ListaZahteva = [];
    this.DataBase.DBGetInfo().subscribe((DataT)=>{
      if(DataT != null)
      {
        this.DataBase.DBVratiPotvrdjene(DataT[0]).subscribe(
          (data) =>
          {
            if(data!= null)
            {
              for(var i = 0, j = 0 ; i <= data.length ; i++,j++)
              {
                  this.ListaZahteva[j] = new Zahtev();  
                  this.ListaZahteva[j].id = data[i++];
                  this.ListaZahteva[j].status = data[i++];
                  this.ListaZahteva[j].id_posiljaoca = data[i++];
                  this.ListaZahteva[j].id_primaoca = data[i++];
                  this.ListaZahteva[j].id_oglasa = data[i];
                  this.GetNames(j,false);
                  this.GetOglasName(j);                     
              }
            }
          } ,
          (error)=>{});        
      }
    } ,(error)=>{});
  }

// Funkcija za odgovaranaj na zahteve
  Odgovori(id_string:string,option:boolean): void
  {
    let id = parseInt(id_string);
    let opt = 0;
    if(option == true)
    opt = 1;
    else
    opt = 0;
    this.DataBase.DBOdgovori(id,opt).subscribe((data)=>{},(error)=>{});
    location.reload();
  }

  // Funkcija za zatvaranje modala
  close(content:any):void 
  { 
    this.modalService.dismissAll(content);
  }

  // Funkcija za otvaranje modala poruka
  openPoruka(content:any): void
  {
    this.modalService.open(content);
  }


  // Otvaranje modala CV-a
  openCV(content:any): void 
  { 
    // Pribavljanje podataka za CV seketovanog korisnika
    this.DataBase.DBGetOtherCV(this.Korisnik.KorisnickoIme).subscribe((data)=>
    {
      if(data!= null) {
        this.SelektovaniCV = new CV();
        this.SelektovaniCV.obrazovanje = data.obrazovanje;
        this.SelektovaniCV.godina_iskustva = data.godina_iskustva;
        this.SelektovaniCV.email = data.email;
        this.SelektovaniCV.tehnologije = data.tehnologije;
        this.SelektovaniCV.telefon = data.telefon;
        this.SelektovaniCV.licni_opis = data.licni_opis;
        this.SelektovaniCV.work_experience = data.work_experience;
        this.SelektovaniCV.jezici = data.jezici;
        this.SelektovaniCV.adresa = data.adresa;  
        this.modalService.open(content); 
      }    
    },(error)=>
    {
      this.close(content);
      alert("Korisnik ne poseduje adekvatan CV !");
    })
  }

  // Otvaranje modala oglasa
  openOglas(content:any,id_string:string): void
    {
     let id = parseInt(id_string);
     this.modalService.open(content);
     this.DataBase.DBGetOglasInfoID(id).subscribe((data)=>
     {
      this.SelektovaniOglas = data[0];
     },(error)=>{});
    }

    // Funkcija za slanje poruka
    posalji(content:Poruka): void
    {
      this.poruka.email=this.Korisnik.Email;
      this.DataBase.DBPosaljiPoruku(this.poruka,this.app.User.KorisnickoIme).subscribe((data)=>
      {
        if(data!=null){
          alert("Poruka uspesno poslata!");
          this.close(content);
          this.app.getToPage('Index');
        }
        else{
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
      }) 
     }


    // Funkcija za otvaranje modala profila korisnika
    open(content:any,id_string:string ) 
    { 
      this.modalService.open(content);
      this.Korisnik = new Korisnik();
      let id = parseInt(id_string);
      this.DataBase.DBOtherInfo(id).subscribe((data)=>{
       switch(data[1])
       {
           case "Privatno lice":
           this.Korisnik.KorisnickoIme = data[0];
           this.Korisnik.tip = data[1];
           this.Korisnik.Email = data[2];
           this.Korisnik.Grad = data[3];
           if(data[4].toLowerCase() == "blank")
           this.Korisnik.Kontakt = "";
           else
           this.Korisnik.Kontakt = data[4]
           this.Korisnik.Ime = data[5];
           this.Korisnik.Prezime = data[6];
           this.Korisnik.imgPath = data[7];      
           break;
 
           case "Firma":
           this.Korisnik.KorisnickoIme = data[0];
           this.Korisnik.tip = data[1];
           this.Korisnik.Email = data[2];
           this.Korisnik.Grad = data[3];
           if(data[4].toLowerCase() == "blank")
           this.Korisnik.Kontakt = "";
           else
           this.Korisnik.Kontakt = data[4]
           this.Korisnik.Naziv = data[5];
           this.Korisnik.Lokacija = data[6];    
           this.Korisnik.imgPath = data[7];        
           break;
 
           case "Radnik":
             this.Korisnik.KorisnickoIme = data[0];
             this.Korisnik.tip = data[1];
             this.Korisnik.Email = data[2];
             this.Korisnik.Grad = data[3];
             this.Korisnik.Ime = data[4];
             this.Korisnik.Prezime = data[5]; 
             this.Korisnik.imgPath = data[6];      
           break;
 
           case "Admin":
             this.Korisnik.KorisnickoIme = data[0];
             this.Korisnik.tip = data[1];
             this.Korisnik.Email = data[2];
             this.Korisnik.Grad = data[3];
             this.Korisnik.Ime = data[4];
             this.Korisnik.Prezime = data[5];
             this.Korisnik.imgPath = data[6];   
           break;
          default:
            alert("fail V");
           break;
       }
     } ,
     (error)=>{});
     this.Korisnik.id = id;
    }

    // Funkcija za kreiranje putanje do slike u bazi podataka
    public createImgPath = (serverPath: string) => { 
      return `https://localhost:5001/${serverPath}`; 
    }
    
}
