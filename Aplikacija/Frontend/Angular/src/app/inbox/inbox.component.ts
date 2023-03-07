import { Component, OnInit } from '@angular/core';
import {NgbModal, ModalDismissReasons} from '@ng-bootstrap/ng-bootstrap';
import { AppService } from '../services/app.service';
import { Poruka } from './Poruka';
import { FormsModule, FormGroup, NgForm ,FormControl} from '@angular/forms';
import { AppComponent } from '../app.component';
import { data } from 'jquery';
import { faTrash } from '@fortawesome/free-solid-svg-icons';
import { CookieService } from 'ngx-cookie-service';

@Component({
  selector: 'app-inbox',
  templateUrl: './inbox.component.html',
  styleUrls: ['./inbox.component.css']
})
export class InboxComponent implements OnInit {

faTrash=faTrash; // Ikonica kante za otpatke
FormaTekst:FormGroup; // Forma za uzimanje teksta poruke

//Pomocne promenjive za UI strane//
  primljene:boolean;
  UserType:boolean;
//------------------------------//

//Promenjive za rad sa porukama//
  poruka:Poruka;
  izabrano:Poruka;
  prPoruke:Poruka[];
  poPoruke:Poruka[];
  RevprPoruke:Poruka[];
  RevpoPoruke:Poruka[];
  selektovanaPoruka:Poruka;
//---------------------------//
  

  constructor(private appService:AppService,private app:AppComponent,private modalService:NgbModal,private cookies:CookieService) 
  { 
    this.prPoruke=[];
    this.poPoruke=[];  
    this.RevprPoruke=[];
    this.RevpoPoruke=[];
    this.UserType = true
    this.primljene = true;
    this.poruka = new Poruka();
    this.izabrano=new Poruka();
    this.selektovanaPoruka=new Poruka(); 
  }

  ngOnInit(): void 
  {
    // Provera da li je korisnik ulogovan
    if(this.cookies.get("Token")=="")
    {
      this.app.IndexPage = true;
      this.app.getToPage("Index");
    }

  // Inicijalizacija forme
    this.FormaTekst = new FormGroup({

      'Tekst': new FormControl(null)

    });

    // Pribavljanje pristiglih poruka iz baze podataka
    this.appService.DBGetInfo().subscribe((data)=>
    {
      if(data != null)
      {  
       if(data[0] != null)
        {
          this.appService.DBVratiPrimljenePoruke(data[0]).subscribe((data)=>
          {
            {
              data.forEach(el=>
                {
      
                  this.RevprPoruke.push(el);
                  
                  this.primljene=true;
      
                });
                this.prPoruke = this.RevprPoruke.reverse();
            }
          },(error)=>{});
        }     
      }
      else{
        this.app.Logout();
      }
    },(error)=>{});
  }

  // Funkcija za prebacivanje pogleda na poslate poruke
  klikPoslate(): void
  { 
    this.poPoruke=[];
    this.RevpoPoruke=[];
    this.primljene=false;

    //Pribavljanje poslatih poruka iz baze podataka
    this.appService.DBVratiPoslatePoruke(this.app.User.KorisnickoIme).subscribe((data)=>
    {
      if(data!=null)
      {
        data.forEach(el => {
          this.RevpoPoruke.push(el);
        });
        this.poPoruke = this.RevpoPoruke.reverse();
      }
      else{
        this.app.Logout();
      }
    },(error)=>{});

  }

  // Funkcija za prebacivanje pogleda na primljene poruke
  klikPrimljene(): void
  {  
    this.prPoruke=[];
    this.RevprPoruke=[];
    this.primljene=true;
    // Pribavljanje primljenih poruka iz baze podataka
    this.appService.DBVratiPrimljenePoruke(this.app.User.KorisnickoIme).subscribe((data)=>
    {
      if(data!=null)
      {
        data.forEach(el=>
          {
            this.RevprPoruke.push(el);
          });
          this.prPoruke = this.RevprPoruke.reverse();
      }
      else{
        this.app.Logout();
      }    
    },(error)=>{});
  }

  // Funkcija za brisanje poruka
  obrisi(event:MouseEvent,poruka:Poruka): void
  {
    event.preventDefault();
    event.stopPropagation();
    if(confirm("Da li ste sigurni?")){
    this.appService.DBObrisiPoruku(poruka.id).subscribe((data)=>{},(error)=>{})
    alert("Poruka obrisana");
    this.app.getToPage('Index');
  }
  else
  alert("Brisanje otkazano")
  this.app.getToPage('Index');
  }

  // Funkcija za otvaranje modala poruke
  open(content:any,obj:Poruka): void
  {
    this.modalService.open(content);
    this.selektovanaPoruka.email=obj.email;
    this.selektovanaPoruka.datum=obj.datum;
    this.selektovanaPoruka.vreme=obj.vreme;
    this.selektovanaPoruka.naslov=obj.naslov;
    this.selektovanaPoruka.poruka=obj.poruka;
    this.selektovanaPoruka.korisnicko_ime=obj.korisnicko_ime;
  }

// Funkcija za zatvaranje modala
  close(content:any):void
  {
    this.modalService.dismissAll(content);
  }

  // Otvara modal za odgovor na postojecu poruku
  openOdgovori(content:any,email:string): void
  {
    this.poruka.email=email;
    this.modalService.open(content);
  }



  posalji(content:Poruka): void
  {
    // Slanje poruke u bazu podataka
    this.appService.DBPosaljiPoruku(this.poruka,this.app.User.KorisnickoIme).subscribe((data)=>
    {
      if(data!=null){
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
    })

   }
}
