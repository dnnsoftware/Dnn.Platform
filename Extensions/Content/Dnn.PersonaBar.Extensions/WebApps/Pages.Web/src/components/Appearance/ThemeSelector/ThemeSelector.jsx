import React, {Component} from "react";
import PropTypes from "prop-types";
import { Label } from "@dnnsoftware/dnn-react-common";
import localization from "../../../localization";
import Card from "../Card/Card";
import Gallery from "../Gallery/Gallery";

class ThemeSelector extends Component {

    onThemeClick(theme) {
        this.props.onSelectTheme(theme);
    }

    isSelected(theme) {
        const { selectedTheme } = this.props;
        if (!selectedTheme) {
            return false;
        }

        return selectedTheme.packageName === theme.packageName && selectedTheme.level === theme.level;
    }

    getSelectedIndex() {
        const { selectedTheme, themes } = this.props;
        if (!selectedTheme) {
            return -1;
        }
        return themes.findIndex(c => c.packageName === selectedTheme.packageName);
    }

    getThemeCards() {
        const { themes } = this.props;
        let {defaultPortalThemeName, defaultPortalThemeLevel} = this.props;
        defaultPortalThemeName = defaultPortalThemeName === null ? "" : defaultPortalThemeName;
        defaultPortalThemeLevel = defaultPortalThemeLevel === null ? "" : defaultPortalThemeLevel;

        if (themes.length === 0) {
            return <div className="no-appearance-items">{localization.get("NoThemes") }</div>;
        }
        return themes.map(theme => {
            return <Card
                key={theme.packageName}
                cardId={theme.packageName}
                onClick={this.onThemeClick.bind(this, theme) }
                hoverText={localization.get("SetPageTheme") }
                label={theme.packageName}
                isSiteDefault={(defaultPortalThemeName.toString().toLowerCase() === theme.packageName.toString().toLowerCase()) && defaultPortalThemeLevel === theme.level }
                selected={this.isSelected(theme) }
                image={theme.thumbnail} />;
        });
    }

    render() {
        const selectedIndex = this.getSelectedIndex();
        return (
            <div>
                <Label
                    label={localization.get("PageTheme") }
                    tooltipMessage={localization.get("PageThemeTooltip") } />
                <Gallery scrollToIndex={selectedIndex}>
                    {this.getThemeCards() }
                </Gallery>
            </div>
        );
    }
}

ThemeSelector.propTypes = {
    selectedTheme: PropTypes.object,
    themes: PropTypes.array.isRequired,
    onSelectTheme: PropTypes.func.isRequired,
    defaultPortalThemeName: PropTypes.string,
    defaultPortalThemeLevel: PropTypes.number
};

export default ThemeSelector;