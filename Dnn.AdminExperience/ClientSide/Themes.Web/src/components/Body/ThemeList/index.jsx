import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { GridCell } from "@dnnsoftware/dnn-react-common";
import { Scrollbars } from "react-custom-scrollbars";
import Localization from "localization";
import Theme from "./Theme";
const NoDataIcon = require("!raw-loader!./../SvgIcon/nodata.svg").default;
import "./style.less";

class ThemeList extends Component {
    constructor() {
        super();
        this.state = {};
    }

    /*eslint-disable react/no-danger*/
    render() {
        const { props } = this;
        let globalThemes = props.dataSource.filter(t => t.level === 4);
        let siteThemes = props.dataSource.filter(t => t.level === 1 || t.level === 2);
        return (
            <div className="theme-list-wrapper">
                {globalThemes.length === 0 && siteThemes.length === 0 &&
                    <div className="empty-state">
                        <div className="noThemes">{Localization.get("NoThemes")}</div>
                        <div className="noThemesMessage">{Localization.get("NoThemesMessage")}</div>
                        <div className="noThemesIcon" dangerouslySetInnerHTML={{ __html: NoDataIcon }} />
                    </div>
                }
                {globalThemes.length > 0 &&
                    <div>
                        <div className="theme-list-title">{Localization.get("GlobalThemes") }</div>
                        <Scrollbars
                            className="theme-list"
                            autoHeight
                            autoHeightMin={0}
                            autoHeightMax={480}>
                            <GridCell>
                                {globalThemes.map((theme, i) => {
                                    return <GridCell key={i} columnSize={25}><Theme theme={theme} /></GridCell>;
                                })}
                            </GridCell>
                        </Scrollbars>
                    </div>
                }
                {globalThemes.length > 0 && siteThemes.length > 0 &&
                    <hr className="theme-list-separator"></hr>
                }
                {siteThemes.length > 0 &&
                    <div>
                        <div className="theme-list-title">{Localization.get("SiteThemes") }</div>
                        <Scrollbars
                            className="theme-list"
                            autoHeight
                            autoHeightMin={0}
                            autoHeightMax={480}>
                            <GridCell>
                                {siteThemes.map((theme, i) => {
                                    return <GridCell key={i} columnSize={25}><Theme theme={theme} /></GridCell>;
                                })}
                            </GridCell>
                        </Scrollbars>
                    </div>
                }
            </div>
        );
    }
}

ThemeList.propTypes = {
    dispatch: PropTypes.func.isRequired,
    dataSource: PropTypes.array
};

function mapStateToProps(state) {
    return {
        currentTheme: state.theme.currentTheme
    };
}

export default connect(mapStateToProps)(ThemeList);