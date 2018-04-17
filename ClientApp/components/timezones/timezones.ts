import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import NewTimeZoneDialogComponent from '../newTimeZoneDialog/newtimezonedialog';

export interface TimeZone {
    id: string;
    name: string;
    city: string;
    owner: string;
    ownerId: string;
    gmtOffset: number;
}

@Component({
    props: ["loginInfo", "filterText"],
    components: {
        NewTimeZoneDialogComponent: require('../newTimeZoneDialog/newtimezonedialog.vue.html')
    }
})
export default class TimeZoneComponent extends Vue {
    timezones: TimeZone[] = []
    loginInfo: any
    loading: boolean = true
    filterText: string

    mounted() {
        console.log(JSON.stringify(this.loginInfo))
        fetch('api/timezones')
            .then(response => {
                if (!response.ok) {
                    throw new Error()
                }

                return response.json() as Promise<TimeZone[]>
            })
            .then(data => {
                this.timezones = data;
                this.loading = false;
                //this.$forceUpdate();
            });
    }

    get filteredTimeZones() {
        return this.timezones.filter(t => { return !this.filterText || t.name.indexOf(this.filterText) !== -1 || t.owner.indexOf(this.filterText) !== -1 })
    }

    newTimeZone() {
        (this.$refs.newTimeZoneDialog as NewTimeZoneDialogComponent).visible = true
    }

    canEdit(timezone: any) {
        return timezone.ownerId == this.loginInfo.id || this.loginInfo.role == "Admin";
    }

    updateName(timezone: any, target: any) {
        if (target.textContent.length < 3) {
            target.textContent = timezone.name
            return
        }

        timezone.name = target.textContent
        fetch('api/timezones/' + timezone.id,
            {
                method: "PUT",
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + this.loginInfo.jwt
                },
                body: JSON.stringify(timezone)
            })
            .then(response => {
                if (!response.ok) {
                    throw new Error()
                }

                return response.text() as Promise<string>
            })
            .then(data => {
                // Success!
            });
    }

    updateCity(timezone: any, target: any) {
        if (target.textContent.length < 2) {
            target.textContent = timezone.city
            return
        }

        timezone.city = target.textContent
        fetch('api/timezones/' + timezone.id,
            {
                method: "PUT",
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + this.loginInfo.jwt
                },
                body: JSON.stringify(timezone)
            })
            .then(response => {
                if (!response.ok) {
                    throw new Error()
                }

                return response.text() as Promise<string>
            })
            .then(data => {
                // Success!
            });
    }

    updateGmtOffset(timezone: any, target: any) {
        console.log(target.textContent)
        var offset: number = Number(target.textContent)
        if (isNaN(offset)) {
            target.textContent = timezone.gmtOffset
            return
        }

        timezone.gmtOffset = target.textContent
        fetch('api/timezones/' + timezone.id,
            {
                method: "PUT",
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + this.loginInfo.jwt
                },
                body: JSON.stringify(timezone)
            })
            .then(response => {
                if (!response.ok) {
                    throw new Error()
                }

                return response.text() as Promise<string>
            })
            .then(data => {
                // Success!
            });
    }

    deleteTimeZone(timezone: any) {
        fetch('api/timezones/' + timezone.id,
            {
                method: "DELETE",
                headers: {
                    'Authorization': 'Bearer ' + this.loginInfo.jwt
                }
            })
            .then(response => {
                if (!response.ok) {
                    throw new Error()
                }
                return response.text() as Promise<string>
            })
            .then(data => {
                // Success
                var idx = this.timezones.indexOf(timezone)
                if (idx > -1) {
                    this.timezones.splice(idx, 1)
                }
            });
    }

    currentTime(timezone: TimeZone) {
        var today = new Date();
        today.setHours(today.getHours() + timezone.gmtOffset);
        return today.toUTCString()
    }
}
