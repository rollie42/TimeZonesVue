import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import { LoginInfo } from '../app/app';

@Component({
    props: ["loginInfo", "canManagerUsers"]
})
export default class NavMenuComponent extends Vue {
    loginInfo?: LoginInfo    

    get canManagerUsers() {
        return this.loginInfo && (this.loginInfo.role == "Admin" || this.loginInfo.role == "Manager")
    }
}
