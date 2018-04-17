import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import * as jwtDecode from "jwt-decode";
import { LoginInfo } from '../app/app';

@Component({    
    props: ["loginInfo"]
})
export default class LoginDialogComponent extends Vue {
    visible: boolean = false
    name: string = ""
    password: string = ""
    loginInfo: LoginInfo
    login() {
        fetch('api/account/login',
            {
                method: "POST",
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ name: this.name, password: this.password })
            })
            .then(response => {
                this.name = ""
                this.password = ""
                if (!response.ok) {
                    throw new Error()
                }

                return response.text() as Promise<string>
            })
            .then(data => {
                var jwtData: any = jwtDecode(data)
                this.loginInfo.name = jwtData.sub
                this.loginInfo.role = jwtData.Role
                this.loginInfo.id = jwtData.UserId
                this.loginInfo.jwt = data
                localStorage.setItem('jwt', data)
                this.visible = false
            });
    }

    cancel() {
        this.visible = false
    }
}

