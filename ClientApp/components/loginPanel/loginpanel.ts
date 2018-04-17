import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import LoginDialogComponent from '../loginDialog/logindialog';
import { LoginInfo } from '../app/app';

@Component({
    components: {
        LoginDialogComponent: require('../loginDialog/logindialog.vue.html')
    },
    props: ["loginDialog", "registerDialog", "loginInfo"]
})
export default class LoginPanelComponent extends Vue {
    loginDialog: any
    registerDialog: any
    loginInfo: LoginInfo        

    get userName() {
        return this.loginInfo.name
    }

    get loggedin() {
        return this.loginInfo.name.length > 0
    }

    showLoginDialog() {
        this.loginDialog.visible = true
    }

    showRegisterDialog() {
        this.registerDialog.visible = true
    }

    logout() {
        this.loginInfo.clear()
        localStorage.removeItem('jwt')
    }
}
