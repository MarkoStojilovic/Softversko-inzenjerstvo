import { Component, Input, TemplateRef ,HostListener  } from '@angular/core';
import {NavigationStart, Router} from '@angular/router';
import { faInstagram } from '@fortawesome/free-brands-svg-icons';
import { faBriefcase  } from '@fortawesome/free-solid-svg-icons';
import {NgbModal, ModalDismissReasons} from '@ng-bootstrap/ng-bootstrap';
import { RegisterUser } from './RegisterUser';
import { HttpClient } from '@angular/common/http';
import { interval, Observable, of,Subscription } from 'rxjs';
import { AppService } from "./services/app.service"
import { CookieService } from 'ngx-cookie-service';

import { faEnvelope} from '@fortawesome/free-solid-svg-icons';
import { faSquare} from '@fortawesome/free-solid-svg-icons';
import { faHandshake } from '@fortawesome/free-solid-svg-icons';
import { Korisnik } from './Korisnik';
import { PlatformLocation } from '@angular/common' 
import { Alert } from 'bootstrap';



export let browserRefresh = false;


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'DvlHub';
  faBriefcase = faBriefcase;

  regexp = new RegExp(/^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/);
  format = /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]+/;

//Pomocne promenjive za UI i funkcionalnosti//
RegisterError:number; // -1 - default  |  0 - Ne poklapaju se lozinke  |  1 - Nije validan mail  |  2 - Nije izabran tip  | 3 - Nije sve popunjeno
LoginStatus:boolean;// Status logina | true - logged in  | false - logged out 
CollapseMenuState:boolean;// Promenjiva za pokazivanje collapse menu-a
RegisterForm:RegisterUser; // Cuva podatke vezane za register podatke
Tip:number // Koristi se za proveru tipa na modalu za register
LoginError:boolean; // Pokazivac da postoji error kod login-a
MenuState:boolean;// Promenjiva koja aktivira collapse menu
oglasDugme:boolean = true; //za disable dugmica
getScreenWidth = window.innerWidth;
User:Korisnik; // Podaci korisnika
ImeTrenutneKomponente!: string;
response!: {dbPath: ''};
EditPassword:boolean;
EditProfile:boolean;
OglasiPage:boolean;
ImageLoad:boolean;
IndexPage:boolean;
//---------------------------------------//

le = window.Comment
//Ikonice//
faEnvelope = faEnvelope;
faSquare = faSquare;
faHandshake = faHandshake;
//------//

 
  constructor(private router:Router,private modalService: NgbModal,private DataBase:AppService,private cookies:CookieService,location: PlatformLocation)
  {
  window.addEventListener('popstate', (event) => {this.getToPage("Index");}, false);
  this.RegisterForm = new RegisterUser();
  this.CollapseMenuState = false;
  this.User = new Korisnik();
  this.EditPassword = false;
  this.EditProfile = false;
  this.LoginStatus = false; 
  this.LoginError = false; 
  this.RegisterError = -1; 
  this.ImageLoad = false;
  this.MenuState = true;
  this.IndexPage = true;
  this.Tip = -1;
  }

  
  ngOnInit(): void 
  {
     this.getScreenWidth = window.innerWidth;
     if(this.getScreenWidth <= 991)
     this.MenuState = false;
     else
       this.MenuState = true;
 
  // Proverava se da li je korisnik ulogovan
   if(this.cookies.get("Token") != "" && this.LoginStatus == false)
   {
    this.LoginStatus = true;
    this.GetInfoDB();
   }
   
   // Periodicno proveravanje validnosti tokena i auto logout
    if(this.LoginStatus == true)
    {
     let TokenCheck= setInterval(() => {
       if(this.cookies.get("Token") == "" && this.LoginStatus == true)
       {
         alert("Vaša prijava je istekla !!!");      
         this.Logout(); 
         this.cookies.deleteAll();
         clearInterval(TokenCheck);
         this.getToPage("Index");
       }
     }, 1800*1000);
    }  

    this.cookies.set("dodajOglas","false"); //dodavanje oglasa 
 
 if(this.User.imgPath == "-1" ||this.User.imgPath == null )
  {
   this.ImageLoad = false;
  }
  else if(this.User.imgPath != "" && this.User.imgPath != " ")
  {
    this.ImageLoad = true;
  }
 
  }
 
  // Event koji pri aktiviranju drugih komponenti gasi pogleda na komponentu index
  onActivate(event: any): void 
  {
   this.ImeTrenutneKomponente = event.constructor.name;
   this.IndexPage = false;
 }
 

  // Listener koji gleda promenu sirine ekrana
  @HostListener('window:resize', ['$event'])
   onWindowResize(): void 
   {
     this.getScreenWidth = window.innerWidth;
     if(this.getScreenWidth <= 991)
     this.MenuState = false;
     else
       this.MenuState = true;
   }
 

  // Otvara ostale komponente
  getToPage(pageName:string): void
  {
    if(pageName == "Index")
    {
     this.oglasDugme=true;
     this.IndexPage = true;
     this.router.navigate([``]);
    }
   
    else
    {
     this.oglasDugme=true;
     this.IndexPage = false;
     this.router.navigate([`${pageName}`]);
    }
  }


//Funkcija koja se aktivira kad se klikne na Dodaj oglas
 dodajOglas(): void  
 {
   this.oglasDugme=false;
   this.cookies.set("dodajOglas","true");
   this.getToPage("Oglas")
 }
 
//Funkcija koja se aktivira kad se klikne na Pogledaj oglase
 pogledajOglase(): void  
 {
   this.oglasDugme=false;
   this.cookies.set("dodajOglas","false");
   this.getToPage('Oglas');
 }
 
  //Funkcija koja otvara modal
  open(content:any): void  
  { 
   this.LoginError = false;
   this.modalService.open(content);
   this.Tip = -1;
   this.RegisterError = -1;
  }
 

 // Funkcija koja zatvara modale
  close(content:any): void 
  { 
   this.modalService.dismissAll(content);
   this.RegisterForm = new RegisterUser();
  }
 
  
 // Funkcija za proveru podataka pri registraciji
  CheckRegisterData(content:any):void
   {
     this.RegisterError = -1;
     if(this.Tip < 0 || this.Tip > 2)
     {
       this.RegisterError = 0; 
       return;
     }
     else if (this.regexp.test(this.RegisterForm.Email) == false)
     {
       this.RegisterError = 1; 
       return;
     }
     else if ((this.RegisterForm.Lozinka == "" || this.RegisterForm.PotvrdaLozinka == "" ) || (this.RegisterForm.Lozinka != this.RegisterForm.PotvrdaLozinka))
     {
       this.RegisterError = 2; 
       return;
     }
 
    // Provera tipa korisnika
     if(this.Tip == 0)
     {
       this.RegisterForm.Tip = "Radnik";
     }
     else if(this.Tip == 1)
     {
       this.RegisterForm.Tip = "Privatno lice";
     }
     else if(this.Tip == 2)
     {
       this.RegisterForm.Tip = "Firma";
     }
 
     // Provera za korisnike
     if(this.RegisterForm.Grad == "" || this.RegisterForm.KorisnickoIme == "" || this.format.test(this.RegisterForm.KorisnickoIme) == true )
     {
       this.RegisterError = 3; 
       return;
     }
 
     //Provera ostalih gresaka za razlicite tipove
     if(this.RegisterForm.Tip == "Radnik" || this.RegisterForm.Tip == "Privatno lice")
     {
       if(this.RegisterForm.Ime == "" || this.RegisterForm.Prezime == "")
       {
         this.RegisterError = 3; 
         return;
       }
     }
     else if(this.RegisterForm.Tip == "Firma" && (this.RegisterForm.Lokacija == ""  || this.RegisterForm.Naziv == "" ))
     {
       this.RegisterError = 3; 
       return;
     }
 
     // Proverava se u bazi da li vec postoje korisnici s sa ovi podacima
     this.DataBase.DBCheckData(this.RegisterForm.KorisnickoIme,this.RegisterForm.Email).subscribe((data)=>
     {   
       if(data == true)
       {
         alert("Neuspešno ste se registovali !");
         alert("Vas email ili korisničko ime se već koristi!");
   
       } 
     else{
       this.Register(this.RegisterForm,content);     
       } 
     });
   }
 
 
 
  // Funkcija koja registruje korisnike u bazi podataka
 Register(obj:RegisterUser,content:any): void 
 {
   this.DataBase.DBRegister(obj).subscribe((data)=>{
     
     if(data == true)
     {
       alert("Uspešno ste se registovali !!!");
       this.RegisterForm = new RegisterUser();
       this.close(content);
     } 
   });
 }
 

 // Funkcija za prijavljivanje korisnika
 User_Login(korisnicko_ime:string,lozinka:string,content:any):void
 {
    this.DataBase.DBLogin(korisnicko_ime,lozinka).subscribe((data)=>
    {
      if(data!= null)
     {
      this.DataBase.DBCheckType(korisnicko_ime,lozinka).subscribe((data2)=>
      {
        if(data2 != null)
        {
          this.cookies.set("Token",data,{ expires: new Date(new Date().getTime() + 1800 * 1000)})
          this.LoginError = false;
          this.close(content);
          this.LoginStatus = true;     
          this.ngOnInit();
          this.getToPage("Index")
          this.GetInfoDB();
      }
      });
     }
     else
     {
      this.LoginError = true;
     }
    });
   }

 
 // Funkcija za odjavljivanje 
 Logout(): void 
 {
   this.User = new Korisnik();
   this.cookies.deleteAll();
   localStorage.clear();
   this.LoginStatus = false;
   this.getToPage("Index");
 }

// Funkcija za aktiviranje i deaktiviranje collapse menu-a
 CollapseMenu(): void 
 {
   if(this.CollapseMenuState == false)
   this.CollapseMenuState = true
   else
   this.CollapseMenuState = false;
 }
 

 // Funkcija za izmenu lozinke korisnika
 EditPass(): void 
 {
   if(this.EditPassword == true) 
   {
     if(this.RegisterForm.Lozinka == "" || this.RegisterForm.Lozinka == "")
     this.RegisterError = 14;
    else if(this.RegisterForm.Lozinka == this.RegisterForm.PotvrdaLozinka)
     this.RegisterError = 10;
     else
     {
       this.RegisterError = -1;
       this.EditPassChange();
       this.DataBase.DBChangePass(this.User.Email,this.RegisterForm.Lozinka,this.RegisterForm.PotvrdaLozinka).subscribe((data=>
         {
           if(data == true)
           alert("Izmena lozinke uspešna!");
           else
           {
             this.RegisterError = 12;
             this.EditPassChange();
           } 
         }),(error)=>{});     
     }  
   }
   else
   this.EditPassChange();
 }

 // Funkcija za aktiviranje UI za izmenu lozinke
 EditPassChange(): void 
 {
   if(this.EditPassword == true )
   {
     this.RegisterError = -1;
     this.EditPassword = false;
   }
   else
   this.EditPassword = true;
 }
 
 // Funkcija za promenu podataka profila korisnika
 EditProfileInfo(): void 
 {
   if(this.EditProfile == true)
   {
     if(this.User.tip == "Radnik" || this.User.tip == "Admin")
     {
       this.EditProfile = false;  
       location.reload();
       this.DataBase.DBChangeInfo(this.User)?.subscribe((data)=> 
       {
         if(data == true ) 
          this.GetInfoDB();  
       },(error)=>{});
     }
     else if(this.regexp.test(this.User.Kontakt) == true)
     {  
       this.EditProfile = false;   
       location.reload();
       this.DataBase.DBChangeInfo(this.User)?.subscribe((data)=> 
       {      
         if(data == true )
          this.GetInfoDB();                             
       },(error)=>{});    
     }
      else
     this.RegisterError = 9;
   }
   else
   this.EditProfile = true;
 }

 // Funkcija za izlazak iz promene podataka
 ExitProfileInfo(): void 
 {
   if(this.EditProfile == true)
   this.EditProfile = false;
   else
   this.EditProfile = true;
   
   this.RegisterError = -1;
   this.GetInfoDB();
   this.RegisterForm = new RegisterUser();
 }
 
 //Funkcija za pribavljanje podataka od ulogovanom korisniku
 GetInfoDB(): void 
 {
   this.DataBase.DBGetInfo().subscribe((data)=>
     {
       if(data == null)
         this.Logout() 
       else
       {
         switch(data[1])
         {
           case "Privatno lice":
             this.User.KorisnickoIme = data[0];
             this.User.tip = data[1];
             this.User.Email = data[2];
             this.User.Grad = data[3];
             if(data[4].toLowerCase() == "blank")
             this.User.Kontakt = "";
             else
             this.User.Kontakt = data[4]
             this.User.Ime = data[5];
             this.User.Prezime = data[6];
             this.User.imgPath = data[7];
             break;
             case "Firma":
             this.User.KorisnickoIme = data[0];
             this.User.tip = data[1];
             this.User.Email = data[2];
             this.User.Grad = data[3];
             if(data[4].toLowerCase() == "blank")
             this.User.Kontakt = "";
             else
             this.User.Kontakt = data[4]
             this.User.Naziv = data[5];
             this.User.Lokacija = data[6];
             this.User.imgPath = data[7];
              break;

             case "Radnik":
               this.User.KorisnickoIme = data[0];
               this.User.tip = data[1];
               this.User.Email = data[2];
               this.User.Grad = data[3];
               this.User.Ime = data[4];
               this.User.Prezime = data[5];
               this.User.imgPath = data[6];
             break;

             case "Admin":
                 this.User.KorisnickoIme = data[0];
                 this.User.tip = data[1];
                 this.User.Email = data[2];
                 this.User.Grad = data[3];
                 this.User.Ime = data[4];
                 this.User.Prezime = data[5];
                 this.User.imgPath = data[6];                  
               break;

            default:
              alert("fail");
             break;
         }
       }
       this.ngOnInit();},(error)=>{})
 }
 
 // Funkcija za ubacivanje slike u bazu podataka
 uploadFinished = (event:any) => { 
   this.response = event;
 }

 // Funkcija za pribavljanje putanje do slike iz baze podataka
 public createImgPath = (serverPath: string) => { 
   return `https://localhost:5001/${serverPath}`; 
 }
 
 }
 