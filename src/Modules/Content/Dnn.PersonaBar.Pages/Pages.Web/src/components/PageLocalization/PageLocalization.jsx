import React, {Component, PropTypes} from "react";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import Button from "dnn-button";
import Localization from "../../localization";
import Scrollbars from "react-custom-scrollbars";
import PageLanguage from "./PageLanguage";
import utils from "../../utils";
import style from "./style.less";

import LanguagesActions from "../../actions/languagesActions";

class PageLocalization extends Component {

    constructor() {
        super();
        this.state = {
            Locales: [],
            Modules: [],
            Pages: []
        };
    }

    componentWillMount() {
        this.getLanguages();
    }

    getLanguages() {
        const {props, state} = this;
        const {tabId} = props.page;
        props.dispatch(LanguagesActions.getLanguages(tabId, (data) => {
            this.setState(data);
            console.log('DATA:', data);
        }));

    }

    makePageTranslatable() {
        const {props, state} = this;
        const {tabId} = props.page;
        props.dispatch(LanguagesActions.makePageTranslatable(tabId, (data) => {
            console.log('DATA:', data);
            this.getLanguages();
        }));
    }

    makePageNeutral() {
        const {props, state} = this;
        const {tabId} = props.page;
        props.dispatch(LanguagesActions.makePageNeutral(tabId, (data) => {
            console.log('DATA:', data);
        }));
    }

    componentWillReceiveProps(newProps) {
    }

    renderPageLanguage(local, modules, page) {
        const Modules = modules && modules.Modules ? modules.Modules : [];
        return <PageLanguage
            local={local}
            modules={Modules}
            page={page}
            />;
    }

    getAllLanguages() {
        const {Locales, Modules, Pages} = this.state;
        const pageLanguages = Locales.map((l, index) => {
            if (!index) {
                return false;
            }
            return this.renderPageLanguage(Locales[index], Modules[index], Pages[index]);
        });
        return pageLanguages;
    }

    render() {
        const {Locales, Modules, Pages} = this.state;
        const Languages = this.getAllLanguages();
        const containerStyle = {width: (Languages.length - 1) * 250}; 

        return <div className="page-localization">
            <div className="page-localization-container">
                <div className="default-language-container">
                    {this.renderPageLanguage(Locales[0], Modules[0], Pages[0]) }
                </div>
                <div className="languages-container">
                    <Scrollbars className="scrollArea content-vertical"
                        autoHeight
                        autoHeightMin={0}
                        autoHeightMax={600}>
                        <div style={containerStyle}>
                            {Languages}
                        </div>
                    </Scrollbars>
                </div>
            </div>
            <Button
                type="secondary"
                onClick={this.makePageTranslatable.bind(this) }>
                {Localization.get("MakePagesTranslatable") }
            </Button>
            <Button
                type="secondary"
                onClick={this.makePageNeutral.bind(this) }>
                {Localization.get("MakePageNeutral") }
            </Button>
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
