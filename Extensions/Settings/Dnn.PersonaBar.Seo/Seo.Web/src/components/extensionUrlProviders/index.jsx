import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    seo as SeoActions
} from "../../actions";
import ProviderRow from "./providerRow";
import ProviderEditor from "./providerEditor";
import GridCell from "dnn-grid-cell";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.less";

let tableFields = [];

class ExtensionUrlProvidersPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            openId: ""
        };
    }

    UNSAFE_componentWillMount() {
        const {props} = this;

        tableFields = [];
        tableFields.push({ "name": resx.get("Name.Header"), "id": "Name" });
        tableFields.push({ "name": resx.get("Enabled.Header"), "id": "Enabled" });

        props.dispatch(SeoActions.getExtensionUrlProviders());
    }

    onUpdateProviderStatus(providerId, isActive) {
        const {props} = this;

        props.dispatch(SeoActions.updateExtensionUrlProviderStatus({ ProviderId: providerId, IsActive: isActive }, () => {
            util.utilities.notify(resx.get("SettingsUpdateSuccess"));
        }, () => {
            util.utilities.notifyError(resx.get("SettingsError"));
        }));
    }

    uncollapse(id) {
        setTimeout(() => {
            this.setState({
                openId: id
            });
        }, this.timeout);
    }

    collapse() {
        if (this.state.openId !== "") {
            this.setState({
                openId: ""
            });
        }
    }

    toggle(openId) {
        if (openId !== "") {
            this.uncollapse(openId);
        } else {
            this.collapse();
        }
    }

    renderProvidersHeader() {
        let tableHeaders = tableFields.map((field) => {
            let className = "provider-items header-" + field.id;
            return <div className={className} key={"header-" + field.id}>
                <span>{field.name}</span>
            </div>;
        });
        return <div className="header-row">{tableHeaders}</div>;
    }

    renderedProviders() {
        if (this.props.providers) {
            if (this.props.providers.length > 0) {
                return this.props.providers.map((item, index) => {
                    return (
                        <ProviderRow
                            providerId={item.ExtensionUrlProviderId}
                            name={item.ProviderName}
                            enabled={item.IsActive}
                            index={index}
                            key={"provider-" + index}
                            closeOnClick={true}
                            openId={this.state.openId}
                            onUpdateStatus={this.onUpdateProviderStatus.bind(this)}
                            OpenCollapse={this.toggle.bind(this)}
                            Collapse={this.collapse.bind(this)}>
                            <ProviderEditor
                                settingUrl={item.SettingUrl}
                                Collapse={this.collapse.bind(this)}
                                openId={this.state.openId} />
                        </ProviderRow>
                    );
                });
            }
            else {
                return <GridCell className="no-extension-url-providers">{resx.get("NoExtensionUrlProviders")}</GridCell>;
            }
        }
    }

    /* eslint-disable react/no-danger */
    render() {
        return (
            <div className={styles.extensionUrlProviders}>
                <div className="columnTitle">{resx.get("ExtensionUrlProviders")}</div>
                <div className="provider-items-grid">
                    {this.renderProvidersHeader()}
                    {this.renderedProviders()}
                </div>
            </div>
        );
    }
}

ExtensionUrlProvidersPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    providers: PropTypes.array
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        providers: state.seo.extensionUrlProviders
    };
}

export default connect(mapStateToProps)(ExtensionUrlProvidersPanelBody);