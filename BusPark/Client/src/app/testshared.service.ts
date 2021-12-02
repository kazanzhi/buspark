import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class TestsharedService {
  readonly APIUrl = "https://localhost:44391/api";

  constructor(private http: HttpClient) {
  }

  rentinfo(val: any) {
    return this.http.get<any>(this.APIUrl + "/rent/rent-info");
  }

  login(val: any) {
    return this.http.post(this.APIUrl + "/rent/login", val);
  }

  register(val: any) {
    return this.http.post(this.APIUrl + "/rent/register", val);
  }

  rentbus(val: any) {
    return this.http.post(this.APIUrl + "/rent/rent-bus", val);
  }

  unrentbus(val: any) {
    return this.http.post(this.APIUrl + "/rent/unrent-bus", val);
  }
}
