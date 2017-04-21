
import {Observable} from 'rxjs'
import {ProvideAntiForgeryToken} from './_provider.anti-forgery-token'


export const GetPortalTabs = ({portalId, cultureCode, isMultiLanguage, excludeAdminTabs, disableNotSelectable, roles, sortOrder}) => {
    const source = Observable.create((observer) => {

        const serviceRoot = dnn.getVar("sf_siteRoot", "/");
        const TabId = dnn.getVar("sf_tabId", -1);
        const RequestVerificationToken  = ProvideAntiForgeryToken()


        let queries = [
            `portalId=${portalId}`,
            `cultureCode=${cultureCode}`,
            `isMultiLanguage=${isMultiLanguage}`,
            `excludeAdminTabs=${excludeAdminTabs}`,
            `disabledNotSelectable=${disableNotSelectable}`,
            `roles=${roles}`,
            `sortOrder=${sortOrder}`
        ]


        const params = {
            url: `/API/PersonaBar/Tabs/GetPortalTabs?${queries.join('&')}`,
            headers:{
                TabId: TabId,
                RequestVerificationToken: RequestVerificationToken
             }
         }

        observer.next()

    })

}
