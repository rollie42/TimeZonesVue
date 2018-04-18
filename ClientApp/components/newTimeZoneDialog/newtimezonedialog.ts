import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import * as jwtDecode from "jwt-decode";
import { TimeZone } from '../timezones/timezones'

@Component({    
    props: ["loginInfo", "timezones"]
})
export default class NewTimeZoneDialogComponent extends Vue {
    visible: boolean = false
    name: string = ""
    city: string = ""
    gmtOffset: number = 0
    loginInfo: any
    timezones: TimeZone[]

    create() {
        //var self = this
        var tz: TimeZone = { id: "", name: this.name, city: this.city, owner: this.loginInfo.name, ownerId: this.loginInfo.id, gmtOffset: this.gmtOffset }

        fetch('api/timezones',
            {
                method: "POST",
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + this.loginInfo.jwt
                },
                body: JSON.stringify(tz)
            })
            .then(response => {
                this.name = ""
                this.city = ""
                this.gmtOffset = 0
                if (!response.ok) {
                    throw new Error()
                }

                return response.json() as Promise<TimeZone>
            })
            .then(data => {
                console.log(tz)
                console.log(data)
                this.visible = false
                this.timezones.push(data)
            });
    }

    cancel() {
        this.visible = false
    }
}

