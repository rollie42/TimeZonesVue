import Vue from 'vue';
import { Component } from 'vue-property-decorator';

interface User {
    id: string;
    name: string;
    role: string;
}

@Component({
    props: ["loginInfo"]
})
export default class FetchDataComponent extends Vue {
    users: User[] = []
    loginInfo: any

    mounted() {
        fetch('api/account/users', {
                method: "GET",
                headers: {
                    'Authorization': 'Bearer ' + this.loginInfo.jwt
                }
            })
            .then(response => {
                if (!response.ok) {
                    throw new Error()
                }

                return response.json() as Promise<User[]>
            })
            .then(data => {
                this.users = data
            });
    }

    updateRole(user: User, target: any) {
        var txt = target.textContent
        var valid = txt == "Admin" || txt == "Manager" || txt == "User"
        if (!valid || (txt == "Admin" && this.loginInfo.role != "Admin")) {            
            target.textContent = user.role
            return
        }

        user.role = target.textContent
        fetch('api/account/users/' + user.id,
            {
                method: "PUT",
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + this.loginInfo.jwt
                },
                body: JSON.stringify(user)
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

    deleteUser(user: User) {
        fetch('api/account/users/' + user.id,
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
                var idx = this.users.indexOf(user)
                if (idx > -1) {
                    this.users.splice(idx, 1)
                }
            });
    }

    canEdit(user: User) {
        return this.loginInfo.role == "Admin" || (this.loginInfo.role == "Manager" && user.role == "User")
    }
}
