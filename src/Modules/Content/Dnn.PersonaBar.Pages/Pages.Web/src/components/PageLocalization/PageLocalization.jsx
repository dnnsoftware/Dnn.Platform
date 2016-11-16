import React, {Component, PropTypes} from "react";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import Button from "dnn-button";
import Localization from "../../localization";
import PageLanguage from "./PageLanguage";
import utils from "../../utils";
import style from "./style.less";

import LanguagesActions from "../../actions/languagesActions";

class PageLocalization extends Component {

    constructor() {
        super();
        this.state = {
            data: {
                Locals: [
                    {CultureCode: "Da-da", Icon: "/images/Flags/da-DK.gif"}
                ],
                Modules: [
                    [{ModuleTitle: "Blu-Ray Players"}, {ModuleTitle: "Smart TV"}]
                ],
                Pages: [
                   {Description: "", Title: "", TabName: "Home"} 
                ]
            }
        };
    }

    componentWillMount() {
        this.getLanguages();
    }

    getLanguages() {
        const {props, state} = this;
        const {tabId} = props.page;
        props.dispatch(LanguagesActions.getLanguages(tabId, (data) => {
            console.log('DATA:', data);
        }));

    }

    componentWillReceiveProps(newProps) {
    }

    renderPageLanguage(local, modules, page) {
        return <PageLanguage
            local={local}
            modules={modules}
            page={page} 
        />;
    }


    render() {
        return <div className="page-localization">
            <div className="page-localization-container">
                <div className="default-language-container">
                    {this.renderPageLanguage(this.state.data.Locals[0], this.state.data.Modules[0], this.state.data.Pages[0])}
                </div>
                <div className="languages-container">
                </div>
            </div>  
        </div>;   
    }
}

PageLocalization.propTypes = {
    page: PropTypes.object.isRequired
};

function mapStateToProps(state) {
    return {
        themes: state.theme.themes,
        defaultPortalThemeName: state.theme.defaultPortalThemeName,
        defaultPortalLayout: state.theme.defaultPortalLayout,
        defaultPortalContainer: state.theme.defaultPortalContainer,
        layouts: state.theme.layouts,
        containers: state.theme.containers
    };
}

export default connect(mapStateToProps)(PageLocalization);
