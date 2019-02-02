import React, { Component } from "react";
import PropTypes from "prop-types";

const helper = {
    renderActions(template, actions) {

        const components = [];
        let actionTemplate = template;

        const tokenRegex = /\{(.+?)\|(.+?)\}/;
        while (tokenRegex.test(actionTemplate)) {
            const match = tokenRegex.exec(actionTemplate);

            components.push(actionTemplate.substr(0, match.index));

            const action = ((type) => {
                if (typeof actions[type] === "function") {
                    return actions[type];
                }

                return null;
            })(match[1]);

            components.push(<strong onClick={action}>{match[2]}</strong>);

            actionTemplate = actionTemplate.substr(match.index + match[0].length);
        }

        if (actionTemplate) {
            components.push(actionTemplate);
        }

        return components;
    }
};

export default helper;