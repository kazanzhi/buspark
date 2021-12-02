import { Component } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { TestsharedService } from './testshared.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  formlogin: FormGroup;
  formregister: FormGroup;
  isLoginState: boolean;
  isLoogedIn: boolean;
  errorMessage: string = '';

  BusList: any = [];

  CurrentRent: any;

  constructor(private formBuilder: FormBuilder, private service: TestsharedService) {
    this.isLoginState = true;

    this.formlogin = this.formBuilder.group({
      username: '',
      password: ''
    })

    this.formregister = this.formBuilder.group({
      username: '',
      password: ''
    })

    this.isLoogedIn = false;

    var state = localStorage.getItem("loggedin") as boolean | null;
    if (state !== null)
      this.isLoogedIn = state;
  }

  ngOnInit(): void {
    this.loadBuses();
  }

  loadBuses(): any {
    var currId = localStorage.getItem("lastId");
    this.service.rentinfo({}).subscribe(data => {
      this.BusList = data;

      let rent = this.BusList.find((x: any) => {
        if (x.driver !== null) {
          return x.driver.id == currId && x.status == "rent";
        }
        else {
          return false;
        }
      });
      if (!rent) {
        this.CurrentRent = null;
      }
      else {
        this.CurrentRent = rent;
      }
    });

  }

  changeState(state: boolean): void {
    this.isLoginState = state;
  }

  rentbus(val: any): void {
    var currId = localStorage.getItem("lastId");
    this.service.rentbus({ driverId: currId, busId: val })
      .subscribe((res: any) => {
        this.errorMessage = "";
      },
        error => {
          console.error("Rent Error", error)
          this.errorMessage = error.error.message;
        });

    window.location.reload();
    this.loadBuses();
  }

  unrentbus(val: any): void {
    var currId = localStorage.getItem("lastId");
    this.service.unrentbus({ driverId: currId, busId: val })
      .subscribe((res: any) => {
        this.errorMessage = "";
      },
        error => {
          console.error("Rent Error", error)
          this.errorMessage = error.error.message;
        });

    window.location.reload();
    this.loadBuses();
  }

  logIn(): void {
    this.service.login(this.formlogin.getRawValue())
      .subscribe((res: any) => {
        this.errorMessage = "";
        this.isLoogedIn = true;
        localStorage.setItem("loggedin", "true");
        localStorage.setItem("lastId", res.id);
        window.location.reload();
      },
        error => {
          console.error("Login Error", error)
          this.errorMessage = error.error.message;
        });
  }

  register(): void {
    this.service.register(this.formregister.getRawValue())
      .subscribe((res: any) => {
        this.errorMessage = "";
        this.isLoogedIn = true;
        localStorage.setItem("loggedin", "true");
        localStorage.setItem("lastId", res.id);
        window.location.reload();
      },
        error => {
          console.error("Register Error", error)
          this.errorMessage = error.error.message;
        });
  }

  logout(): void {
    localStorage.removeItem("loggedin");
    localStorage.removeItem("lastId");
    this.isLoogedIn = false;
  }
}
