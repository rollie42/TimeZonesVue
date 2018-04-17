import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import * as jwtDecode from "jwt-decode";
import LoginDialogComponent from '../loginDialog/logindialog';

@Component({
    components: {
        MenuComponent: require('../navmenu/navmenu.vue.html'),
        LoginPanelComponent: require('../loginPanel/loginpanel.vue.html'),
        LoginDialogComponent: require('../loginDialog/logindialog.vue.html'),
        RegisterDialogComponent: require('../registerDialog/registerdialog.vue.html')
    }
})
export default class AppComponent extends Vue {
    loginInfo: LoginInfo = new LoginInfo()
    mounted() {
        // See if we have a JWT in localstorage already
        var jwt = localStorage.getItem('jwt')
        if (jwt) {
            var jwtData: any = jwtDecode(jwt)
            this.loginInfo.name = jwtData.sub
            this.loginInfo.role = jwtData.Role
            this.loginInfo.id = jwtData.UserId
            this.loginInfo.jwt = jwt
        }
        this.$forceUpdate();
    }
}

export class LoginInfo {
    name: string = ""
    id: string = ""
    role: string = "" 
    jwt: string = ""

    clear() {
        this.name = ""
        this.id = ""
        this.role = ""
        this.jwt = ""
    }
}
