import React, {Component, PropTypes} from "react";
import Label from "dnn-label";
import localization from "../../../localization";
import Card from "../Card/Card";
import Gallery from "../Gallery/Gallery";

class ThemeSelector extends Component {

    onCardClick(cardId) {
        console.log("clicked on " + cardId);
    }

    getThemeCards() {
        return this.props.themes.map(theme => {
            return <Card 
                cardId={theme.packageName}
                onClick={this.onCardClick.bind(this)}
                hoverText={localization.get("ClickSelectTheme")}
                label={theme.packageName}
                image={theme.thumbnail} />;
        });
    }

    render() {        
        return (
            <div>
                <Label 
                    label={localization.get("PageTheme")} 
                    tooltipMessage={localization.get("PageThemeTooltip")} />
                <Gallery>
                    {this.getThemeCards()}
                </Gallery>
            </div>
        );
    }
}

ThemeSelector.propTypes = {
    currentTheme: PropTypes.object.isRequired,
    themes: PropTypes.array.isRequired
};

export default ThemeSelector;