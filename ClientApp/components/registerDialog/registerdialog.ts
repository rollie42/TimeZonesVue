import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import * as jwtDecode from "jwt-decode";

@Component({    
    props: ["loginInfo"]
})
export default class RegisterDialogComponent extends Vue {
    visible: boolean = false
    name: string = ""
    password: string = ""
    loginInfo: any
    register() {
        fetch('api/account/register',
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
                console.log('response = ' + JSON.stringify(jwtData))
                this.loginInfo.name = jwtData.sub
                this.loginInfo.role = jwtData.Role
                this.loginInfo.id = jwtData.UserId
                this.loginInfo.jwt = data
                this.visible = false
            });
    }

    cancel() {
        this.visible = false
    }
}

