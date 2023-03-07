import { Component, Input, OnInit } from '@angular/core';
import {NgbModal, ModalDismissReasons} from '@ng-bootstrap/ng-bootstrap';
import { AppService } from '../services/app.service';
import { CV } from './CV';
import { FormGroup, FormControl } from '@angular/forms';
import { CookieService } from 'ngx-cookie-service';
import {Router} from '@angular/router';
import { AppComponent } from '../app.component';


@Component({
  selector: 'app-cv',
  templateUrl: './cv.component.html',
  styleUrls: ['./cv.component.css']
})


export class CVComponent implements OnInit {
regexp = new RegExp(/^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/); // Za proveru email formata

//---Forme za prikupljanje teksta---//
FormaTehnologije!:FormGroup;
FormaObrazovanje!:FormGroup;
FormaIskusvo!:FormGroup;
FormaJezici!:FormGroup;
FormaRezime!:FormGroup;
//--------------------------------- //

// Pomocne promenjive za realizaciju UI//
CV_data:CV;
EditMode:boolean;
UserType:boolean;
ErrorState:number;
ImeKorisnika:string;
PrezimeKorisnika:string;
//----------------------------------- //

  constructor(private Database:AppService,public cookies:CookieService,private router:Router,private  app:AppComponent) 
  { 
   this.ErrorState = -1;
   this.EditMode = false;
   this.UserType = false;
   this.ImeKorisnika = "";
   this.CV_data = new CV();
   this.PrezimeKorisnika = "";
  }

  ngOnInit(): void 
  {
// Provera da li je korisnik ulogovan
    this.CheckActivity();

// Inicijalizacija formi
    this.FormaTehnologije = new FormGroup({
      'Tehnologije': new FormControl(null)
    });
    this.FormaJezici = new FormGroup({
      'Jezici': new FormControl(null)
    });

   this.FormaIskusvo = new FormGroup({
      'Iskustvo': new FormControl(null)
    });
    this.FormaObrazovanje = new FormGroup({
      'Obrazovanje': new FormControl(null)
    });
    this.FormaRezime = new FormGroup({
      'Rezime': new FormControl(null)
    });

    
    // Provera korisnika pri ulazu i pronalazenje CV-a korisnika
    this.Database.DBGetInfo().subscribe((data)=>
    {
      if(data != null)
      {
       if(data[1] == "Radnik")
        {
          this.UserType = true;
          this.ImeKorisnika = data[4];
          this.PrezimeKorisnika = data[5];
          // Prikupljanje podata za CV 
          this.Database.DBGetCV(data[0]).subscribe((data)=>
          {
            if(data != null)
            {            
              this.CV_data.email =  data.email;             
              this.CV_data.adresa = data.adresa;
              this.CV_data.jezici =  data.jezici;
              this.CV_data.telefon = data.telefon;         
              this.CV_data.licni_opis = data.licni_opis;
              this.CV_data.obrazovanje = data.obrazovanje;
              this.CV_data.tehnologije = data.tehnologije;
              this.CV_data.work_experience = data.work_experience; 
              this.CV_data.godina_iskustva =  data.godina_iskustva;     
            }
          },
          (error)=>{});   
        }
        else
        {
          this.app.IndexPage = true;
          this.app.getToPage("Index");
          this.UserType = false;
        }
      }
      else
      {
       this.app.Logout();
      }
    },(error)=>{}
    );
 }
 
 //Funkcija za pokretanje Edit-a CV-a
  EditText(): void
  {
   
    this.CheckActivity();
    this.EditMode = true;
    this.ErrorState = -1;
  }

  //Memorisanje CV-a
  SaveText(): void
  {
  this.CheckActivity();
  this.ErrorState = -1;

  this.CV_data.jezici = this.FormaJezici.get('Jezici')?.value;
  this.CV_data.licni_opis = this.FormaRezime.get("Rezime")?.value;
  this.CV_data.work_experience =this.FormaIskusvo.get('Iskustvo')?.value;
  this.CV_data.obrazovanje = this.FormaObrazovanje.get('Obrazovanje')?.value;
  this.CV_data.tehnologije = this.FormaTehnologije.get('Tehnologije')?.value;
  
  if(this.CV_data.tehnologije == "" || this.CV_data.jezici == "" || this.CV_data.work_experience == "" || this.CV_data.obrazovanje  == ""  || this.CV_data.licni_opis == "" || this.CV_data.adresa == "" ||  this.CV_data.telefon == "")
  this.ErrorState = 1;
  if(this.regexp.test(this.CV_data.email) == false)
  this.ErrorState = 2;
  if(this.CV_data.godina_iskustva < 0 || this.CV_data.godina_iskustva > 40 )
  this.ErrorState = 3;

  if(this.ErrorState <0)
  {
    this.Database.DBGetInfo().subscribe((data)=>
    {
      if(data != null)
      {
       if(data[1] == "Radnik")
        {
        this.Database.DBAddCV(this.CV_data,data[0]).subscribe((data2)=>{},(error)=>{});
        this.EditMode = false;
        }
      }
      else
      {
       this.app.Logout();
      }
    },(error)=>{}
    )
  }
   
}

//Provera da li je korisnik ulogovan
CheckActivity(): void
  {
    if(this.cookies.get("Token") == "")
    {
      this.app.IndexPage = true;
      this.getToPage("");
    }  
  }

  // Prelazak na drugu stranu
  getToPage(pageName:string):void
 {
  this.router.navigate([`${pageName}`]);
 }

// Kreiranje putanje slike u bazi
 public createImgPath = () => { 
  let  serverPath = this.app.User.imgPath;
  return `https://localhost:5001/${serverPath}`; 
  }

// Provera da li je putanja slike validna
  public RetImg()
  {
   if(this.app.User.imgPath == null)
    return false;
   else
    return true;
  }
}
