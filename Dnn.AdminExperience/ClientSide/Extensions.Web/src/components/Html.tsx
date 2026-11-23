import React from "react";
import parse, { attributesToProps, domToReact, Element } from "html-react-parser";
import type { DOMNode, HTMLReactParserOptions } from "html-react-parser";
import DOMPurify from "dompurify";

// DOMPurify strips "target", so links inside the Persona Bar iframe would navigate the iframe itself.
// Applied after sanitizing, so only these fixed attributes are added to already sanitized links.
const newWindowLinkOptions: HTMLReactParserOptions = {
    replace(domNode) {
        if (domNode instanceof Element && domNode.name === "a") {
            return (
                <a {...attributesToProps(domNode.attribs)} target="_blank" rel="noopener noreferrer">
                    {domToReact(domNode.children as DOMNode[], newWindowLinkOptions)}
                </a>
            );
        }
    }
};

export default function Html({html, openLinksInNewWindow = false}) {
    const sanitizedHtml = DOMPurify.sanitize(html);
    return openLinksInNewWindow ? parse(sanitizedHtml, newWindowLinkOptions) : parse(sanitizedHtml);
}
