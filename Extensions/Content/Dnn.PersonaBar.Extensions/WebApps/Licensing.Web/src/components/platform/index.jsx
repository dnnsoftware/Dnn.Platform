import React, { Component } from "react";
import "./style.less";
import resx from "../../resources";
import styles from "./style.less";

/*eslint-disable quotes*/
const starIcon = require(`!raw-loader!./../svg/star_circle.svg`).default;
const evoqIcon = require(`!raw-loader!./../svg/evoq.svg`).default;
const infoIcon = require(`!raw-loader!./../svg/info_circle.svg`).default;

class Platform extends Component {
    constructor() {
        super();
    }

    renderHeader() {
        let tableFields = [];
        tableFields.push({ "name": resx.get("LicenseType.Header"), "id": "LicenseType" });
        tableFields.push({ "name": resx.get("InvoiceNumber.Header"), "id": "InvoiceNumber" });
        tableFields.push({ "name": resx.get("WebServer.Header"), "id": "WebServer" });
        tableFields.push({ "name": resx.get("Activated.Header"), "id": "Activated" });
        tableFields.push({ "name": resx.get("Expires.Header"), "id": "Expires" });

        let tableHeaders = tableFields.map((field) => {
            let className = "header-" + field.id;
            return <div className={className} key={"header-" + field.id}>
                <span>{field.name}</span>
            </div>;
        });
        return <div className="licenses-header-row">{tableHeaders}</div>;
    }

    onEvoqClick() {
        window.open("http://www.dnnsoftware.com/cms-features", "_blank");
    }

    onUpgradeClick() {
        window.open("http://www.dnnsoftware.com/about/contact-dnn", "_blank");
    }

    onDocCenterClick() {
        window.open("http://www.dnnsoftware.com/docs/", "_blank");
    }

    /* eslint-disable react/no-danger */
    renderLinks() {
        return (
            <div className="links-wrapper">
                <div className="link-evoq-wrapper">
                    <div className="link-evoq" onClick={this.onEvoqClick.bind(this) }>
                        <div className="star-icon" dangerouslySetInnerHTML={{ __html: starIcon }} />
                        <div className="link-evoq-header">{resx.get("CheckOutEvoq.Header") }</div>
                        <div className="link-evoq-desc">{resx.get("CheckOutEvoq") }</div>
                    </div>
                </div>
                <div className="link-upgrade-wrapper">
                    <div className="link-upgrade" onClick={this.onUpgradeClick.bind(this) }>
                        <div className="evoq-icon" dangerouslySetInnerHTML={{ __html: evoqIcon }} />
                        <div className="link-upgrade-header">{resx.get("UpgradeToEvoq.Header") }</div>
                        <div className="link-upgrade-desc">{resx.get("UpgradeToEvoq") }</div>
                    </div>
                </div>
                <div className="link-doc-wrapper" onClick={this.onDocCenterClick.bind(this) }>
                    <div className="link-doc">
                        <div className="info-icon" dangerouslySetInnerHTML={{ __html: infoIcon }} />
                        <div className="link-doc-header">{resx.get("DocumentCenter.Header") }</div>
                        <div className="link-doc-desc">{resx.get("DocumentCenter") }</div>
                    </div>
                </div>
            </div>
        );
    }

    /*eslint no-mixed-spaces-and-tabs: "error"*/
    render() {
        return (
            <div className={styles.licensingPlatform}>
                <div>
                    {this.renderHeader() }
                    <div className="nolicense">
                        <div className="nolicense-header">{resx.get("NoLicense.Header") }</div>
                        <div className="nolicense-body">{resx.get("NoLicense") }</div>
                    </div>                
                </div>
                {this.renderLinks() }
            </div>
        );
    }
}

export default Platform;