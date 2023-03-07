import { HttpClient, HttpEventType, HttpErrorResponse } from '@angular/common/http';
import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { AppService } from '../services/app.service';
import { CookieService } from 'ngx-cookie-service';
import { AppComponent } from '../app.component';

@Component({
  selector: 'app-upload',
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.css']
})
export class UploadComponent implements OnInit {
  progress!: number;
  message!: string;
  @Output() public onUploadFinished = new EventEmitter();
  
  constructor(private http: HttpClient,private Database:AppService,private cookies:CookieService,private app:AppComponent) {}

  ngOnInit() {}

  uploadFile = (files:any) => 
  {
    if (files.length === 0) {
      return;
    }

    let fileToUpload = <File>files[0];
    const formData = new FormData();
    formData.append('file', fileToUpload, fileToUpload.name);
    console.log(formData);
    
this.Database.DBGetInfo().subscribe((data)=>
{
  if(data != null)
  {
    this.http.post(`https://localhost:5001/Korisnik/Upload/${data[0]}`, formData, {reportProgress: true, observe: 'events'})
    .subscribe({
      next: (event:any) => {
        this.onUploadFinished.emit(event.body);
    }
  });
  }
})
  

    
  }
}
