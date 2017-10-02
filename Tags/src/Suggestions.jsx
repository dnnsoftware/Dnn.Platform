import React, {Component, PropTypes} from "react";
import {Scrollbars} from "react-custom-scrollbars";

const style = {
    list: {
        height: 170,
        border: "1px solid #E3E3E3",
        backgroundColor: "#FFFFFF"
    }
};

export default class Suggestions extends Component {

    onComponent

    onSelectSuggestion(tag) {
        this.props.onSelectSuggestion(tag);
    }

    onScrollUpdate(scroll) {
        if (typeof (this.props.onScrollUpdate) === "function") {
            this.props.onScrollUpdate(scroll);
        }
    }

    getSuggestions() {
        let suggestions = this.props.suggestions.map((suggestion, index) => {
            let className = this.props.selectedIndex > -1 && index === this.props.selectedIndex ? 'selected' : '';
            return <div className={`suggestion ${className}`} key={index}
                        onClick={this.onSelectSuggestion.bind(this, suggestion.value)}>{suggestion.value}</div>;
        });
        return suggestions;
    }

    render() {
        const suggestions = this.getSuggestions();

        const selectedIndex = this.props.selectedIndex;
        return (<div className="suggestions">
            <Scrollbars ref={(sb) => {
                if(sb) {
                    const items = document.getElementsByClassName('suggestion');
                    if(items) {
                        const viewHeight = sb.getClientHeight();
                        const h = items[0].clientHeight;
                        const selectedItemScroll = h * selectedIndex;
                        const currentScroll = sb.getScrollTop();
                        setTimeout(() => {
                            if(selectedItemScroll < currentScroll) {
                                sb.scrollTop(currentScroll - viewHeight);
                            } else if (selectedItemScroll + h > (currentScroll + viewHeight )) {
                                sb.scrollTop(selectedItemScroll + h + currentScroll + viewHeight);
                            }
                        });
                    }
                }
            }}
                        style={style.list}
                        onUpdate={this.onScrollUpdate.bind(this)}>
                {suggestions}
            </Scrollbars>
        </div>);
    }
}

Suggestions.propTypes = {
    onSelectSuggestion: PropTypes.func.isRequired,
    suggestions: PropTypes.array,
    onScrollUpdate: PropTypes.func,
    selectedIndex: PropTypes.number.isRequired
};

Suggestions.defaultProps = {
    suggestions: [],
    selectedIndex:-1
};