import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { security as SecurityActions } from "../../actions";
import BulletinItemRow from "./bulletinItemRow";
import "./style.less";
import resx from "../../resources";
import styles from "./style.less";

/*eslint-disable quotes*/
const warningIcon = require(`!raw-loader!./../svg/error.svg`).default;

class SecurityBulletinsPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            error: undefined
        };
    }

    UNSAFE_componentWillMount() {
        const {props} = this;
        if (props.securityBulletins) {
            return;
        }
        props.dispatch(SecurityActions.getSecurityBulletins(() => {
        }, (error) => {
            const errorMessage = JSON.parse(error.responseText);
            this.setState({
                error: errorMessage.Message
            });
        }));
    }

    renderHeader() {
        const tableFields = [
            { "name": resx.get("Bulletins"), "id": "Bulletins" }
        ];
        let tableHeaders = tableFields.map((field, i) => {
            let className = "bulletinsHeader bulletinsHeader-" + field.id;
            return <div className={className} key={i}>
                <span>{field.name}</span>
            </div>;
        });

        return <div className="bulletinsHeader-wrapper">{tableHeaders}</div>;
    }

    renderedList() {
        const {props} = this;
        if (props.securityBulletins) {
            return props.securityBulletins.map((term, index) => {
                return (
                    <BulletinItemRow
                        className="bulletin-detail-wrapper"
                        pubDate={term.PubDate}
                        title={term.Title}
                        link={term.Link}
                        description={term.Description}
                        author={term.Author}
                        index={index}
                        key={"bulletin-" + index}
                        closeOnClick={true}>
                        <div className="bulletin-detail">
                            <div style={{ fontWeight: "bolder", margin: "0 0 15px 0" }}>{resx.get("BulletinDescription")}</div>
                            <div className="bulletin-detail-desc">
                                {term.Description}
                            </div>
                            <div style={{ fontWeight: "bolder", margin: "15px 0 15px 0" }}>{resx.get("BulletinLink")}</div>
                            <div className="bulletin-detail-link">
                                <a target="_blank" href={term.Link} rel="noopener noreferrer">{term.Link}</a>
                            </div>
                            <div style={{ height: "20px" }}>&nbsp;</div>
                        </div>
                    </BulletinItemRow>
                );
            });
        }
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        if (state.error) {
            return (
                <div className={styles.bulletins}>
                    <div className="warning-container">
                        <div className="warning-icon" dangerouslySetInnerHTML={{ __html: warningIcon }} />
                        <div className="warning-msg">{state.error}</div>
                    </div>
                </div>
            );
        }
        else {
            if (props.securityBulletins && props.securityBulletins.length > 0) {
                return (
                    <div className={styles.bulletins}>
                        {this.renderHeader()}
                        {this.renderedList()}
                    </div>
                );
            }
            else {
                return <div style={{ margin: "25px" }}>{resx.get("BulletinsDoNotExist").replace("{0}", props.platformVersion)}</div>;
            }
        }
    }
}

SecurityBulletinsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    platformVersion: PropTypes.string,
    securityBulletins: PropTypes.object
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        platformVersion: state.security.platformVersion,
        securityBulletins: state.security.securityBulletins
    };
}

export default connect(mapStateToProps)(SecurityBulletinsPanelBody);